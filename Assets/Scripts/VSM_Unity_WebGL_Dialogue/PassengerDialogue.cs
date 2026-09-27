using System;
using System.Collections.Generic;
using UnityEngine;

public class PassengerDialogue : MonoBehaviour
{
    [SerializeField] private GameScenario scenario;

    public class ChatMessage
    {
        public string Speaker;
        public string Text;
    }
    private readonly List<ChatMessage> history = new();
    public IReadOnlyList<ChatMessage> History => history;
    public GameAssessmentData Assessment { get; private set; }
    public string LastError { get; private set; }
    public bool HasStarted => started;
    public bool HasScenario => scenario != null && !string.IsNullOrEmpty(scenario.code);
    public bool IsConcluded => concluded || IsFinished;
    public bool CanInteract => HasScenario && !IsFinished;
    public bool CanFinish => started && (hasDecision || DeadlineExpired || concluded);
    private bool hasDecision;
    public GameScenario Scenario => scenario;
    private string Endpoint => $"/api/game/sessions/{Uri.EscapeDataString(VSMGameSession.Instance.SessionId)}/scenarios/{Uri.EscapeDataString(scenario.code)}";
    private bool DeadlineExpired => !string.IsNullOrEmpty(scenario?.decisionDeadlineAt) &&
        DateTimeOffset.TryParse(scenario.decisionDeadlineAt, out var end) && DateTimeOffset.UtcNow >= end;
    public float TimerFraction
    {
        get
        {
            if (!HasScenario || IsConcluded) return 0;
            // The server times the FIRST decision, not the whole conversation.
            if (!string.IsNullOrEmpty(scenario.decisionReceivedAt)) return 1;
            if (scenario.decisionTimeLimitSeconds <= 0 ||
                !DateTimeOffset.TryParse(scenario.decisionDeadlineAt, out var deadline)) return 1;
            return Mathf.Clamp01((float)(deadline - DateTimeOffset.UtcNow).TotalSeconds / scenario.decisionTimeLimitSeconds);
        }
    }

    public bool IsBusy { get; private set; }
    public bool IsFinished { get; private set; }
    private bool started;
    private bool concluded;

    public string CurrentText { get; private set; }
    public string CurrentEmotion { get; private set; }
    public bool ShouldContinue { get; private set; }

    public event Action<string,string> OnPassengerMessage;
    public event Action<string> OnEmotionChanged;
    public event Action<GameAssessmentData> OnDialogueFinished;
    public event Action<string> OnError;

    public void SetScenario(GameScenario value)
    {
        if (IsBusy) return;
        history.Clear();
        Assessment = null;
        LastError = null;
        scenario = value;
        hasDecision = false;
        started = false;
        concluded = value != null && (value.status == "completed" || value.status == "assessed");
        IsFinished = false;
        CurrentText = "";
        CurrentEmotion = "";
        ShouldContinue = false;
    }

    // Вызывается при взаимодействии игрока с пассажиром.
    public void OpenDialogue()
    {
        if (IsBusy || IsFinished) return;
        if (started && (string.IsNullOrEmpty(LastError) || concluded))
        {
            if (ShouldContinue) OnPassengerMessage?.Invoke(CurrentText, CurrentEmotion);
            else { LastError = null; IsBusy = true; Conclude(); }
            return;
        }
        if (VSMGameSession.Instance == null || !VSMGameSession.Instance.IsReady)
        {
            Error("Ожидаем авторизацию сайта и загрузку сценариев. Повторите попытку.");
            return;
        }
        if (!HasScenario)
        {
            Error("Пассажиру не назначен сценарий.");
            return;
        }

        LastError = null;
        IsBusy = true;
        // GET is safe to retry after a lost start response or a scene reload.
        StartCoroutine(VSMApiClient.Instance.Get(Endpoint, RestoreOrStart, ApiError));
    }

    private void RestoreOrStart(string response)
    {
        var r = JsonUtility.FromJson<GameHistoryResponse>(response);
        if (r?.data?.scenario == null || r.data.scenario.code != scenario.code)
        { Error("Не удалось восстановить сценарий пассажира."); return; }
        scenario = r.data.scenario;
        if (scenario.status == "pending")
        {
            StartCoroutine(VSMApiClient.Instance.PostJson(Endpoint + "/start", "", ReceiveDialogue, ApiError));
            return;
        }
        history.Clear();
        hasDecision = !string.IsNullOrEmpty(scenario.decisionReceivedAt);
        foreach (var turn in r.data.history ?? Array.Empty<GameHistoryMessage>())
        {
            bool player = turn.role == "player";
            if (player) hasDecision = true;
            string text = string.IsNullOrEmpty(turn.content) ? turn.action : turn.content;
            if (!string.IsNullOrEmpty(text))
                history.Add(new ChatMessage { Speaker = player ? "Вы" : "Пассажир", Text = text });
            if (!player) CurrentText = text;
        }
        started = true;
        concluded = scenario.status == "completed" || scenario.status == "assessed";
        ShouldContinue = !concluded;
        if (scenario.status == "assessed" && r.data.assessment != null)
        {
            CompleteAssessment(new GameAssessmentData { scenario = scenario, result = r.data.assessment });
            return;
        }
        if (concluded) { Assess(); return; }
        IsBusy = false;
        OnPassengerMessage?.Invoke(CurrentText, CurrentEmotion);
    }

    // Этот метод вызывает UI другого скрипта.
    public bool SendPlayerAnswer(string answer)
    {
        if (IsBusy || !started || IsFinished || !ShouldContinue || string.IsNullOrWhiteSpace(answer) || answer.Trim().Length > 4000)
            return false;

        IsBusy = true;
        LastError = null;
        var request = new PlayerAnswer { answer = answer.Trim() };

        StartCoroutine(VSMApiClient.Instance.PostJson(
            Endpoint + "/respond",
            JsonUtility.ToJson(request),
            response =>
            {
                // Record the turn only when the backend acknowledges it.
                hasDecision = true;
                history.Add(new ChatMessage { Speaker = "Вы", Text = request.answer });
                ReceiveDialogue(response);
            },
            ApiError));
        return true;
    }

    private void ReceiveDialogue(string response)
    {
        if (ReadTerminalAssessment(response)) return;
        var r = JsonUtility.FromJson<GameDialogueResponse>(response);

        if (r?.data?.result == null || r.data.scenario == null || r.data.scenario.code != scenario.code)
        {
            Error("Некорректный ответ AI.");
            return;
        }

        scenario = r.data.scenario;
        started = true;
        IsBusy = false;
        CurrentText = r.data.result.passengerReply;
        CurrentEmotion = r.data.result.situationState;
        ShouldContinue = r.data.result.shouldContinue;

        history.Add(new ChatMessage { Speaker = "Пассажир", Text = CurrentText });
        OnPassengerMessage?.Invoke(CurrentText, CurrentEmotion);
        OnEmotionChanged?.Invoke(CurrentEmotion);

        if (!ShouldContinue)
        {
            IsBusy = true;
            Conclude();
        }
    }

    public void FinishDialogue()
    {
        if (IsBusy || !CanFinish || IsFinished) return;
        LastError = null;
        IsBusy = true;
        ShouldContinue = false;
        Conclude();
    }

    private void Conclude()
    {
        if (concluded) { Assess(); return; }
        StartCoroutine(VSMApiClient.Instance.PostJson(
            Endpoint + "/conclude", "", response =>
            {
                if (ReadTerminalAssessment(response)) return;
                var r = JsonUtility.FromJson<GameConclusionResponse>(response);
                if (r?.data?.result == null || !r.data.result.completed ||
                    r.data.scenario == null || r.data.scenario.code != scenario.code)
                { Error("Сервер не подтвердил завершение сценария."); return; }
                scenario = r.data.scenario;
                concluded = true;
                ShouldContinue = false;
                if (!string.IsNullOrEmpty(r.data.result.passengerReply))
                {
                    CurrentText = r.data.result.passengerReply;
                    history.Add(new ChatMessage { Speaker = "Пассажир", Text = CurrentText });
                    OnPassengerMessage?.Invoke(CurrentText, CurrentEmotion);
                }
                Assess();
            }, ApiError));
    }

    private void Assess()
    {
        StartCoroutine(VSMApiClient.Instance.PostJson(
            Endpoint + "/assess", "", response =>
            {
                var r = JsonUtility.FromJson<GameAssessmentResponse>(response);

                if (r?.data?.result == null || r.data.scenario == null || r.data.scenario.code != scenario.code)
                {
                    Error("Не удалось получить оценку.");
                    return;
                }

                CompleteAssessment(r.data);
            }, ApiError));
    }

    private bool ReadTerminalAssessment(string response)
    {
        // Timeout responses from respond/conclude contain a cached assessment,
        // not a DialogueResult / ConclusionResult (backend timedOutResponse).
        var r = JsonUtility.FromJson<GameAssessmentResponse>(response);
        if (r?.data?.scenario?.status != "assessed" || r.data.scenario.code != scenario.code || r.data.result == null)
            return false;
        CompleteAssessment(r.data);
        return true;
    }

    private void CompleteAssessment(GameAssessmentData data)
    {
        scenario = data.scenario;
        IsBusy = false;
        IsFinished = true;
        concluded = true;
        ShouldContinue = false;
        Assessment = data;
        VSMGameSession.Instance?.MarkAssessed(scenario);
        OnDialogueFinished?.Invoke(data);
    }

    private void OnDestroy()
    {
        if (HasScenario && !IsFinished) VSMGameSession.Instance?.ReturnScenario(scenario);
    }

    private void ApiError(long code, string body)
    {
        Error($"API {code}: {body}");
    }

    private void Error(string message)
    {
        IsBusy = false;
        LastError = message;
        Debug.LogError(message);
        OnError?.Invoke(message);
    }

}
