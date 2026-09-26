using UnityEngine;

// Standalone chat entry point; a scene character is optional.
public class VSMDialogueWindow : MonoBehaviour
{
    private static VSMDialogueWindow instance;
    private PassengerDialogue standalone;
    private PassengerDialogue passenger;
    private PlayerController player;
    private string answer = "", result = "";
    private bool visible, waitForSession, sending;
    private CursorLockMode previousLock;
    private bool previousVisible;
    private Vector2 scroll;

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;
    }

    private void OpenStandalone()
    {
        if (standalone == null)
            standalone = gameObject.AddComponent<PassengerDialogue>();
        Open(standalone);
    }

    public static void Open(PassengerDialogue target)
    {
        if (instance == null)
            instance = new GameObject("VSM Dialogue Window").AddComponent<VSMDialogueWindow>();
        if (instance.visible || target == null) return;
        if (instance.passenger != target) instance.answer = "";
        instance.passenger = target;
        instance.result = "";
        target.OnPassengerMessage += instance.Message;
        target.OnError += instance.Error;
        target.OnDialogueFinished += instance.Finished;
        if (target.Assessment != null) instance.Finished(target.Assessment);
        instance.visible = true;
        MenuInputGate.Acquire(instance);
        instance.previousLock = Cursor.lockState;
        instance.previousVisible = Cursor.visible;
        instance.player = FindAnyObjectByType<PlayerController>();
        if (instance.player != null) instance.player.BlockMoveAndLook(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        instance.waitForSession = VSMGameSession.Instance == null || !VSMGameSession.Instance.IsReady;
        if (!instance.waitForSession) target.OpenDialogue();
    }

    private void Message(string text, string emotion)
    {
        if (sending) { answer = ""; sending = false; }
        scroll.y = float.MaxValue;
    }
    private void Error(string error) { sending = false; }
    private void Finished(GameAssessmentData data)
    {
        result = $"Оценка: {data.result.score}\nБезопасность: {data.result.safetyScore}\nЛояльность: {data.result.loyaltyScore}\n{data.result.outcome}\n{data.result.summary}";
        scroll.y = float.MaxValue;
    }

    private void Close()
    {
        if (passenger != null)
        {
            passenger.OnPassengerMessage -= Message;
            passenger.OnError -= Error;
            passenger.OnDialogueFinished -= Finished;
        }
        visible = false;
        MenuInputGate.Release(this);
        waitForSession = false;
        if (player != null) player.BlockMoveAndLook(false);
        Cursor.lockState = previousLock;
        Cursor.visible = previousVisible;
    }

    private void Update()
    {
        if (visible && passenger == null) Close();
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (!visible) OpenStandalone();
            else if (!passenger.IsBusy) Close();
        }
        if (visible && waitForSession && VSMGameSession.Instance != null && VSMGameSession.Instance.IsReady)
        {
            waitForSession = false;
            passenger.OpenDialogue();
        }
    }
    private void OnDestroy() { if (visible) Close(); }

    private void OnGUI()
    {
        if (!visible)
        {
            if (GUI.Button(new Rect(16, 16, 240, 42), "Чат с пассажиром (F2)")) OpenStandalone();
            return;
        }
        if (passenger == null) return;
        float width = Mathf.Min(680, Screen.width - 24);
        GUILayout.BeginArea(new Rect((Screen.width - width) / 2, 24, width, Screen.height - 48), GUI.skin.box);
        GUILayout.Label("Чат с пассажиром");
        var session = VSMGameSession.Instance;
        var label = new GUIStyle(GUI.skin.label) { wordWrap = true };
        scroll = GUILayout.BeginScrollView(scroll);
        foreach (var turn in passenger.History) GUILayout.Label(turn.Speaker + ": " + turn.Text, label);
        if (!string.IsNullOrEmpty(passenger.CurrentEmotion)) GUILayout.Label(passenger.CurrentEmotion, label);
        if (!string.IsNullOrEmpty(result)) GUILayout.Label(result, label);
        if (!string.IsNullOrEmpty(passenger.LastError)) GUILayout.Label(passenger.LastError, label);
        if (session != null && !string.IsNullOrEmpty(session.LastError)) GUILayout.Label(session.LastError, label);
        GUILayout.EndScrollView();

        if (VSMApiClient.Instance == null || !VSMApiClient.Instance.HasToken)
            GUILayout.Label("Ожидаем токен авторизации.");
        else if (session == null || !session.IsReady)
            GUILayout.Label("Подключение и загрузка сценариев…");
        else if (passenger.IsBusy) GUILayout.Label("Ожидаем ответ сервера…");

        GUI.enabled = !passenger.IsBusy && !passenger.IsFinished && passenger.ShouldContinue;
        answer = GUILayout.TextArea(answer, 4000, GUILayout.Height(90));
        if (GUILayout.Button("Отправить") && !string.IsNullOrWhiteSpace(answer))
        {
            sending = true;
            if (!passenger.SendPlayerAnswer(answer)) sending = false;
        }
        GUI.enabled = !passenger.IsBusy && (session == null || !session.IsLoading);
        if (!passenger.IsFinished && (!passenger.HasStarted || !string.IsNullOrEmpty(passenger.LastError)) &&
            GUILayout.Button("Повторить подключение / загрузку"))
        {
            if (session != null && !session.IsReady)
            {
                waitForSession = true;
                session.CreateOrResumeSession();
            }
            else passenger.OpenDialogue();
        }
        if (passenger.IsFinished && passenger == standalone && session != null && session.Available.Count > 0 &&
            GUILayout.Button("Следующий диалог"))
        {
            passenger.SetScenario(null);
            result = "";
            answer = "";
            passenger.OpenDialogue();
        }
        if (GUILayout.Button("Закрыть")) Close();
        GUI.enabled = true;
        GUILayout.EndArea();
    }
}
