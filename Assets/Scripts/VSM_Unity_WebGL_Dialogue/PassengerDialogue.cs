using System;
using UnityEngine;

public class PassengerDialogue : MonoBehaviour
{
    [SerializeField] private GameScenario scenario;

    public string CurrentText { get; private set; }
    public string CurrentEmotion { get; private set; }
    public bool ShouldContinue { get; private set; }

    public event Action<string,string> OnPassengerMessage;
    public event Action<string> OnEmotionChanged;
    public event Action<GameAssessmentData> OnDialogueFinished;
    public event Action<string> OnError;

    public void SetScenario(GameScenario value)
    {
        scenario = value;
        CurrentText = "";
        CurrentEmotion = "";
        ShouldContinue = false;
    }

    // Вызывается при взаимодействии игрока с пассажиром.
    public void OpenDialogue()
    {
        if (scenario == null)
        {
            Error("Пассажиру не назначен сценарий.");
            return;
        }

        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{scenario.code}/start";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint, "", ReceiveDialogue, ApiError));
    }

    // Этот метод вызывает UI другого скрипта.
    public void SendPlayerAnswer(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return;

        var request = new PlayerAnswer { answer = answer };

        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{scenario.code}/respond";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint,
            JsonUtility.ToJson(request),
            ReceiveDialogue,
            ApiError));
    }

    private void ReceiveDialogue(string response)
    {
        var r = JsonUtility.FromJson<GameDialogueResponse>(response);

        if (r?.data?.result == null)
        {
            Error("Некорректный ответ AI.");
            return;
        }

        CurrentText = r.data.result.passengerReply;
        CurrentEmotion = r.data.result.situationState;
        ShouldContinue = r.data.result.shouldContinue;

        OnPassengerMessage?.Invoke(CurrentText, CurrentEmotion);
        OnEmotionChanged?.Invoke(CurrentEmotion);

        if (!ShouldContinue)
            Conclude();
    }

    private void Conclude()
    {
        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{scenario.code}/conclude";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint, "", response => Assess(), ApiError));
    }

    private void Assess()
    {
        string endpoint =
            $"/api/game/sessions/{VSMGameSession.Instance.SessionId}" +
            $"/scenarios/{scenario.code}/assess";

        StartCoroutine(VSMApiClient.Instance.PostJson(
            endpoint, "", response =>
            {
                var r = JsonUtility.FromJson<GameAssessmentResponse>(response);

                if (r?.data?.result == null)
                {
                    Error("Не удалось получить оценку.");
                    return;
                }

                OnDialogueFinished?.Invoke(r.data);
            }, ApiError));
    }

    private void ApiError(long code, string body)
    {
        Error($"API {code}: {body}");
    }

    private void Error(string message)
    {
        Debug.LogError(message);
        OnError?.Invoke(message);
    }
}
