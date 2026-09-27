using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
class Program
{
    static int checks;
    static void Check(bool ok, string description) { if (!ok) throw new Exception(description); checks++; }
    static VSMApiClient Api => VSMApiClient.Instance;
    static GameScenario Scenario(string code, string state = "pending") => new() { code = code, status = state, decisionTimeLimitSeconds = 60 };
    static string PathFor(string code) => "/api/game/sessions/session/scenarios/" + code;
    static object History(GameScenario s) => new { data = new { scenario = s, history = new[] { new { role = "passenger", content = "Здравствуйте" } } } };
    static object Reply(GameScenario s, bool more = true) => new { data = new { scenario = s, result = new { passengerReply = "Ответ", shouldContinue = more, situationState = "neutral" } } };
    static void Main()
    {
        var session = new GameObject().AddComponent<VSMGameSession>();
        var ten = Enumerable.Range(0,10).Select(i => Scenario("case-"+i)).ToArray();
        Api.Expect("POST", "/api/game/sessions", new { data = new { id = "session" } });
        Api.Expect("GET", "/api/game/sessions/session/scenarios", new { data = new { scenarios = ten } });
        session.CreateOrResumeSession();
        Check(session.IsReady && session.Available.Count == 10, "Ten scenarios loaded");
        var station = new GameObject().AddComponent<StationController>();
        typeof(StationController).GetField("baseTimer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(station, new BaseTimer());
        TestHost.Call(station, "Start");
        TestHost.Call(station, "Update");
        Check(TripCompletionView.Shows == 0 && Time.timeScale == 1, "Incomplete trip does not show completion");
        var controller = new GameObject().AddComponent<ControllerPeople>();
        var people = new List<GameObject>();
        for (int i=0;i<5;i++) { var p = new GameObject(); p.AddComponent<PassengerDialogue>().SetScenario(new GameScenario()); people.Add(p); controller.AddPeople(p); }
        Check(people.All(p => !p.GetComponent<PassengerDialogue>().HasScenario), "Passengers wait for controller/session binding");
        TestHost.Call(controller,"Start");
        Check(people.Count(p=>p.GetComponent<PassengerDialogue>().HasScenario)==3, "Three scenario passengers, two ordinary");
        var first = people[0].GetComponent<PassengerDialogue>();
        var code = first.Scenario.code; var path = PathFor(code);
        Check(first.TimerFraction==1 && first.CanInteract && !first.CanFinish, "Pending passenger marker and finish guard");
        Api.Expect("GET",path,History(Scenario(code)));
        Api.Expect("POST",path+"/start",Reply(Scenario(code,"started")));
        first.OpenDialogue();
        int requests=Api.Requests.Count;
        first.OpenDialogue();
        Check(Api.Requests.Count==requests, "Reopen never starts a second scenario");
        Check(!first.CanFinish, "Cannot conclude before a decision");
        var timed=Scenario(code,"started"); timed.decisionDeadlineAt=DateTimeOffset.UtcNow.AddSeconds(30).ToString("O");
        Api.Expect("POST",path+"/respond",Reply(timed));
        Check(first.SendPlayerAnswer("Помогу вам"), "Answer accepted");
        Check(first.CanFinish && first.TimerFraction > .45f && first.TimerFraction <= .5f, "Server deadline drives timer");
        Api.Expect("POST",path+"/conclude",new { data=new { scenario=Scenario(code,"completed"), result=new { completed=true, passengerReply="Спасибо" } } });
        Api.Expect("POST",path+"/assess",null,true);
        first.FinishDialogue();
        Check(first.IsConcluded && !first.IsFinished && first.TimerFraction==0, "Timer disappears even if assessment temporarily fails");
        Api.Expect("POST",path+"/assess",new { data=new { scenario=Scenario(code,"assessed"), result=new { score=85 } } });
        first.OpenDialogue();
        Check(first.IsFinished && !first.CanInteract && session.CompletedCount==1, "Assessment retry does not repeat conclude");
        session.MarkAssessed(first.Scenario);
        Check(session.CompletedCount==1, "Completion counted once");
        TestHost.Call(controller,"Update");
        Check(people.Count(p => p.GetComponent<PassengerDialogue>() is PassengerDialogue d && d.HasScenario && !d.IsFinished) == 3,
            "Completed scenario immediately replaced without waiting for a station");
        int remaining = session.Available.Count;
        TestHost.Call(controller,"Update");
        Check(session.Available.Count == remaining, "Repeated frames do not consume extra scenarios");
        var seen = new HashSet<string> { code };
        // Complete remaining people and simulate station departures/arrivals until all ten are used.
        for(int wave=0; wave<10 && !session.TripComplete; wave++)
        {
            foreach(var person in people.ToArray())
            {
                var d=person.GetComponent<PassengerDialogue>();
                if(d!=null && d.HasScenario && !d.IsFinished)
                {
                    string c=d.Scenario.code; Check(seen.Add(c),"Scenario never assigned to a second passenger");
                    Api.Expect("GET",PathFor(c),new { data=new { scenario=Scenario(c,"assessed"), history=Array.Empty<GameHistoryMessage>(), assessment=new { score=90 } } });
                    d.OpenDialogue();
                }
                if(d==null || !d.HasScenario || d.IsFinished)
                {
                    controller.RemovePeople(person); people.Remove(person);
                }
            }
            while(people.Count<5) { var p=new GameObject(); p.AddComponent<PassengerDialogue>().SetScenario(new GameScenario()); people.Add(p);controller.AddPeople(p); }
        }
        Check(seen.Count==10 && session.TripComplete && session.Available.Count==0,"Exactly ten unique passengers over trip");
        Check(people.All(p=>!p.GetComponent<PassengerDialogue>().HasScenario),"New passengers ordinary after ten scenarios");
        DialogueUI.IsScenarioVisible = true;
        TestHost.Call(station, "Update");
        Check(TripCompletionView.Shows == 1 && !DialogueUI.IsScenarioVisible && StationController.IsTripEnded,
            "Tenth assessment opens final screen even with chat visible");
        Check(Time.timeScale == 0 && MenuInputGate.IsBlocked && TripCompletionView.Assessment == DialogueUI.CurrentAssessmentText,
            "Completion pauses movement and preserves the last assessment");
        TestHost.Call(station, "Update");
        Check(TripCompletionView.Shows == 1, "Completion screen opens only once");
        TestHost.Call(station, "OnDestroy");
        Check(Time.timeScale == 1 && !MenuInputGate.IsBlocked && !StationController.IsTripEnded, "Leaving completed trip restores input and time");
        var assessmentText = AssessmentText.Format(new AssessmentResult {
            score = 80, summary = "Рекомендация", strengths = new[] { "Помощь" }, mistakes = new[] { "Задержка" },
            criteria = new[] { new AssessmentCriterion { criterion = "Вежливость", score = 5, comment = "Хорошо" } }
        });
        Check(assessmentText.Contains("Рекомендация") && assessmentText.Contains("Помощь") &&
            assessmentText.Contains("Задержка") && assessmentText.Contains("Вежливость: 5") && assessmentText.Contains("Хорошо"),
            "Final assessment keeps recommendations and criterion comments");
        var empty = new GameObject().AddComponent<PassengerDialogue>();
        requests=Api.Requests.Count; empty.OpenDialogue();
        Check(Api.Requests.Count==requests && !empty.HasScenario,"Unassigned person cannot consume scenarios");
        // Reloaded started scenario restores its saved conversation without POST start.
        var restored=new GameObject().AddComponent<PassengerDialogue>();restored.SetScenario(Scenario("resume","started"));
        Api.Expect("GET",PathFor("resume"),new { data=new { scenario=Scenario("resume","started"),history=new[] { new { role="player",content="Решение" },new {role="passenger",content="Ответ"} } } });
        restored.OpenDialogue();
        Check(restored.History.Count==2 && restored.CanFinish && restored.ShouldContinue,"Started dialogue restored");
        // Server timeouts use the assessment envelope directly on /respond.
        Api.Expect("POST",PathFor("resume")+"/respond",new { data=new { scenario=Scenario("resume","assessed"), result=new { score=0, outcome="timed_out", summary="Время истекло" } } });
        restored.SendPlayerAnswer("Позднее решение");
        Check(restored.IsFinished && restored.Assessment.result.outcome=="timed_out" && restored.TimerFraction==0,
            "Timeout result does not trigger invalid conclude request");
        Check(Api.Responses.Count==0,"All expected requests executed");
        Console.WriteLine($"PASS: {checks} scenario-flow checks");
    }
}
