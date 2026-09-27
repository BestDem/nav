using System.Collections.Generic;
using UnityEngine;

public class ControllerPeople : MonoBehaviour
{
    public static ControllerPeople singltonePeople { get; private set; }
    [SerializeField] private BaseTimer baseT;
    private readonly List<GameObject> passengers = new();
    private VSMGameSession session;
    private string lastAssignmentState;

    private void Awake() { singltonePeople = this; }
    private void Start() { RefreshAssignments(); }
    private void Update() { RefreshAssignments(); }

    private void RefreshAssignments()
    {
        // Rebind after late authentication / script reload, and replenish as soon
        // as a dialogue ends instead of waiting for a passenger spawn event.
        if (session != VSMGameSession.Instance)
        {
            if (session != null) session.OnSessionReady -= AssignScenarios;
            session = VSMGameSession.Instance;
            if (session != null) session.OnSessionReady += AssignScenarios;
        }
        AssignScenarios();
    }
    public void AddPeople(GameObject people)
    {
        passengers.Add(people);
        if (people.GetComponent<HappyPeople>() == null) people.AddComponent<HappyPeople>();
        AssignScenarios();
    }
    public void RemovePeople(GameObject people) { passengers.Remove(people); }

    private void AssignScenarios()
    {
        if (session == null || !session.IsReady)
        {
            LogAssignmentState(session == null ? "Ожидаем игровую сессию." : "Ожидаем загрузку сценариев с сервера.");
            return;
        }
        passengers.RemoveAll(p => p == null);
        int active = passengers.FindAll(p => p.GetComponent<PassengerDialogue>() != null &&
            p.GetComponent<PassengerDialogue>().HasScenario && !p.GetComponent<PassengerDialogue>().IsFinished).Count;
        LogAssignmentState($"Пассажиров: {passengers.Count}; со сценарием: {active}; не назначено: {session.Available.Count}; завершено: {session.CompletedCount}/10.");
        int limit = Mathf.Max(3, baseT != null ? baseT.MaxEvilPeople : 3);
        foreach (var person in passengers)
        {
            if (active >= limit || session.Available.Count == 0) break;
            // One scenario per person, including after completion.
            var dialogue = person.GetComponent<PassengerDialogue>();
            if (dialogue != null && (dialogue.HasScenario || dialogue.IsBusy || dialogue.IsFinished)) continue;
            if (dialogue == null) dialogue = person.AddComponent<PassengerDialogue>();
            dialogue.SetScenario(session.TakeRandomScenario());
            Debug.Log($"[VSM Passengers] Появился пассажир со сценарием: {dialogue.Scenario.code} — {dialogue.Scenario.title}. Наведитесь на него и нажмите F (до 3 м).", person);
            var happy = person.GetComponent<HappyPeople>();
            if (happy != null) { happy.enabled = false; Destroy(happy); }
            if (person.GetComponent<PeopleDIalogue>() == null) person.AddComponent<PeopleDIalogue>();
            active++;
        }
    }
    private void LogAssignmentState(string message)
    {
        if (message == lastAssignmentState) return;
        lastAssignmentState = message;
        Debug.Log("[VSM Passengers] " + message, this);
    }

    private void OnDestroy()
    {
        if (session != null) session.OnSessionReady -= AssignScenarios;
        if (singltonePeople == this) singltonePeople = null;
    }
}
