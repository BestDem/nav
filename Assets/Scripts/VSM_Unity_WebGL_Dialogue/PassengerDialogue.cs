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
        started = false;
        concluded = false;
        IsFinished = false;
        CurrentText = "";
        CurrentEmotion = "";
        ShouldContinue = false;
    }

    // Вызывается при взаимодействии игрока с пассажиром.
    public void OpenDialogue()
    {
        if (IsBusy || IsFinished) return;
        if (started)
        {
            if (ShouldContinue) OnPassengerMessage?.Invoke(CurrentText, CurrentEmotion);
            else { IsBusy = true; Conclude(); }
            return;
        }
        if (VSMGameSession.Instance == null || !VSMGameSession.Instance.IsReady)
        {
            Error("Ожидаем авторизацию сайта и загрузку сценариев. Повторите попытку.");
            return;
        }
        if (scenario == null || string.IsNullOrEmpty(scenario.code))
            scenario = VSMGameSession.Instance.TakeRandomScenario();
        if (scenario == null)
        {
            Error("Пассажиру не назначен сценарий.");
            return;
        }

        LastError = null;
        IsBusy = true;
        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{Uri.EscapeDataString(scenario.code)}/start";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint, "", ReceiveDialogue, ApiError));
    }

    // Этот метод вызывает UI другого скрипта.
    public bool SendPlayerAnswer(string answer)
    {
        if (IsBusy || !started || IsFinished || !ShouldContinue || string.IsNullOrWhiteSpace(answer))
            return false;

        IsBusy = true;
        LastError = null;
        var request = new PlayerAnswer { answer = answer.Trim() };

        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{Uri.EscapeDataString(scenario.code)}/respond";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint,
            JsonUtility.ToJson(request),
            response =>
            {
                // Record the turn only when the backend acknowledges it.
                history.Add(new ChatMessage { Speaker = "Вы", Text = request.answer });
                ReceiveDialogue(response);
            },
            ApiError));
        return true;
    }

    private void ReceiveDialogue(string response)
    {
        var r = JsonUtility.FromJson<GameDialogueResponse>(response);

        if (r?.data?.result == null)
        {
            Error("Некорректный ответ AI.");
            return;
        }

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
        if (IsBusy || !started || IsFinished) return;
        LastError = null;
        IsBusy = true;
        ShouldContinue = false;
        Conclude();
    }

    private void Conclude()
    {
        if (concluded) { Assess(); return; }
        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{Uri.EscapeDataString(scenario.code)}/conclude";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint, "", response => { concluded = true; Assess(); }, ApiError));
    }

    private void Assess()
    {
        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{Uri.EscapeDataString(scenario.code)}/assess";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint, "", response =>
            {
                var r = JsonUtility.FromJson<GameAssessmentResponse>(response);

                if (r?.data?.result == null)
                {
                    Error("Не удалось получить оценку.");
                    return;
                }

                IsBusy = false;
                IsFinished = true;
                Assessment = r.data;
                OnDialogueFinished?.Invoke(r.data);
            }, ApiError));
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
