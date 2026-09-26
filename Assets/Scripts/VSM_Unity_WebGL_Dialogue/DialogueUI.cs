using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    private static DialogueUI active;
    private static int escapeFrame = -1;
    public static bool IsScenarioVisible => active != null && active.visible;
    public static bool ConsumesEscapeThisFrame => IsScenarioVisible || escapeFrame == Time.frameCount;
    private ScenarioChatView view;
    private PassengerDialogue passenger;
    private bool configured, visible, submitted, listening, attemptedStart;
    private string pending = "", notice = "";
    private int renderedCount = -1;
    private int renderedWidth;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void VSM_StartDictation(string objectName);
    [DllImport("__Internal")] private static extern void VSM_StopDictation();
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        active = null;
        escapeFrame = -1;
        SceneManager.sceneLoaded -= WireScene;
        SceneManager.sceneLoaded += WireScene;
    }
    private static void WireScene(Scene scene, LoadSceneMode mode)
    {
        // Also finds DIalogue when its old settings canvas starts inactive.
        foreach (var results in Resources.FindObjectsOfTypeAll<DialogueResultsUI>())
        {
            if (results.gameObject.scene != scene || results.name != "DIalogue") continue;
            var ui = results.GetComponent<DialogueUI>();
            if (ui == null) ui = results.gameObject.AddComponent<DialogueUI>();
            ui.ConfigureStandalone();
        }
    }

    public void ConfigureStandalone()
    {
        if (configured) return;
        configured = true;
        active = this;
        var oldInput = GetComponentInChildren<TMP_InputField>(true);
        var font = oldInput != null ? oldInput.fontAsset : TMP_Settings.defaultFontAsset;
        for (int i = 0; i < transform.childCount; i++) transform.GetChild(i).gameObject.SetActive(false);

        // Move the scenario out of the settings hierarchy. It owns its own canvas.
        transform.SetParent(null, false);
        gameObject.name = "VSM_SCENARIO_CHAT";
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = .5f;
        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        gameObject.SetActive(true);
        view = new ScenarioChatView(transform, font);
        view.Send.onClick.AddListener(SendButton);
        view.Input.onSubmit.AddListener(Submit);
        view.Voice.onClick.AddListener(ToggleVoice);
        view.Finish.onClick.AddListener(Finish);
        view.Hide.onClick.AddListener(Hide);
        view.Retry.onClick.AddListener(Retry);
        view.Next.onClick.AddListener(Next);
        passenger = GetComponent<PassengerDialogue>();
        if (passenger == null) passenger = gameObject.AddComponent<PassengerDialogue>();
        Bind(passenger);
        view.Screen.SetActive(false);
    }

    private void Bind(PassengerDialogue target)
    {
        Unbind(); passenger = target;
        passenger.OnPassengerMessage += Message;
        passenger.OnDialogueFinished += Finished;
        passenger.OnError += Error;
    }
    private void Unbind()
    {
        if (passenger == null) return;
        passenger.OnPassengerMessage -= Message;
        passenger.OnDialogueFinished -= Finished;
        passenger.OnError -= Error;
    }
    public void Open(PassengerDialogue target)
    {
        if (!configured) ConfigureStandalone();
        Bind(target);
        attemptedStart = target.HasStarted;
        renderedCount = -1;
        Show();
    }
    private void Show()
    {
        visible = true;
        view.Screen.SetActive(true);
        MenuInputGate.Acquire(this);
        renderedCount = -1;
    }
    private void Hide()
    {
        StopVoice();
        visible = false;
        view.Screen.SetActive(false);
        MenuInputGate.Release(this);
    }

    private void Update()
    {
        if (!configured) return;
        if (Input.GetKeyDown(KeyCode.Escape) && visible)
        {
            escapeFrame = Time.frameCount;
            Hide();
        }
        if (Input.GetKeyDown(KeyCode.F) && Time.timeScale > 0 && (!visible || !view.Input.isFocused))
        {
            if (visible) Hide(); else Show();
        }
        var session = VSMGameSession.Instance;
        if (visible && session != null && session.IsReady && !attemptedStart)
        {
            attemptedStart = true;
            passenger.OpenDialogue();
        }
        if (!visible) return;
        bool ready = !passenger.IsBusy && !passenger.IsFinished && passenger.ShouldContinue;
        view.Input.readOnly = !ready || listening;
        view.Send.interactable = ready && !listening && !string.IsNullOrWhiteSpace(view.Input.text);
#if UNITY_WEBGL && !UNITY_EDITOR
        view.Voice.interactable = ready;
#else
        view.Voice.interactable = false;
#endif
        view.Finish.interactable = !passenger.IsBusy && (passenger.HasStarted || passenger.IsFinished);
        view.FinishLabel.text = passenger.IsFinished ? "Закрыть" : "Завершить";
        view.Next.gameObject.SetActive(passenger.IsFinished && session != null && session.Available.Count > 0);
        view.Retry.gameObject.SetActive(!passenger.IsBusy && !listening &&
            (!string.IsNullOrEmpty(passenger.LastError) || !string.IsNullOrEmpty(session?.LastError)));
        if (listening) view.Status.text = "Слушаю… Нажмите микрофон ещё раз, чтобы остановить.";
        else if (!string.IsNullOrEmpty(notice)) view.Status.text = notice;
        else if (passenger.IsBusy) view.Status.text = passenger.ShouldContinue ? "Пассажир отвечает…" : "Ожидаем ответ сервера…";
        else if (!string.IsNullOrEmpty(passenger.LastError)) view.Status.text = passenger.LastError;
        else if (!string.IsNullOrEmpty(session?.LastError)) view.Status.text = session.LastError;
        else if (session == null || !session.IsReady) view.Status.text = VSMApiClient.Instance != null && VSMApiClient.Instance.HasToken ? "Загружаем сценарий…" : "Ожидаем авторизацию…";
        else if (passenger.Assessment != null)
        {
            var r = passenger.Assessment.result;
            view.Status.text = $"Диалог завершён · Оценка: {r.score}\nБезопасность: {r.safetyScore} · Лояльность: {r.loyaltyScore}\n{r.summary}";
        }
        else view.Status.text = "";
        RenderHistory();
    }

    private void RenderHistory()
    {
        if (renderedCount == passenger.History.Count && renderedWidth == Screen.width) return;
        renderedCount = passenger.History.Count;
        renderedWidth = Screen.width;
        view.ClearMessages();
        foreach (var turn in passenger.History) view.AddMessage(turn.Text, turn.Speaker != "Пассажир");
        if (submitted) view.AddMessage(pending, true, " · ОТПРАВЛЯЕТСЯ");
    }
    private void Submit(string text) { SendButton(); }
    public void SendButton()
    {
        if (passenger == null || listening || passenger.IsBusy || string.IsNullOrWhiteSpace(view.Input.text)) return;
        notice = "";
        pending = view.Input.text;
        submitted = true;
        if (!passenger.SendPlayerAnswer(pending)) submitted = false;
        renderedCount = -1;
    }
    private void Message(string text, string emotion)
    {
        if (submitted) view.Input.text = "";
        submitted = false;
        pending = "";
        renderedCount = -1;
    }
    private void Error(string error)
    {
        submitted = false;
        renderedCount = -1;
    }
    private void Finished(GameAssessmentData data) { renderedCount = -1; }
    private void Finish()
    {
        if (passenger.IsFinished) { Hide(); return; }
        StopVoice();
        notice = "";
        passenger.FinishDialogue();
    }
    private void Retry()
    {
        notice = "";
        var session = VSMGameSession.Instance;
        if (session == null) return;
        if (!session.IsReady) { attemptedStart = false; session.CreateOrResumeSession(); }
        else if (passenger.ShouldContinue && !string.IsNullOrWhiteSpace(view.Input.text)) SendButton();
        else passenger.OpenDialogue();
    }
    private void Next()
    {
        passenger.SetScenario(null);
        notice = ""; pending = ""; submitted = false;
        view.Input.text = "";
        attemptedStart = false;
        renderedCount = -1;
    }

    private void ToggleVoice()
    {
        if (listening) { StopVoice(); return; }
#if UNITY_WEBGL && !UNITY_EDITOR
        notice = "";
        listening = true;
        VSM_StartDictation(gameObject.name);
#else
        return;
#endif
    }
    private void StopVoice()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (listening) VSM_StopDictation();
#endif
        listening = false;
    }
    // Called only by the WebGL speech plugin. Dictation never submits the message.
    public void OnDictationText(string text)
    {
        if (!visible || !listening || string.IsNullOrWhiteSpace(text)) return;
        view.Input.text = (view.Input.text + " " + text).Trim();
        notice = "Текст распознан. Проверьте его и нажмите «Отправить».";
    }
    public void OnDictationEnd(string unused) { listening = false; }
    public void OnDictationError(string code)
    {
        listening = false;
        notice = code == "not-allowed" || code == "service-not-allowed" ? "Разрешите доступ к микрофону в браузере."
            : code == "unsupported" ? "Этот браузер не поддерживает голосовой ввод. Используйте клавиатуру."
            : code == "no-speech" ? "Речь не распознана. Нажмите микрофон и попробуйте ещё раз."
            : "Не удалось распознать речь. Проверьте микрофон и подключение к сети.";
    }
    private void OnGUI()
    {
        if (configured && !visible && Time.timeScale > 0 && GUI.Button(new Rect(20, 20, 230, 40), "Открыть сценарий · F")) Show();
    }
    private void OnDisable() { StopVoice(); MenuInputGate.Release(this); }
    private void OnEnable() { if (visible) MenuInputGate.Acquire(this); }
    private void OnDestroy() { Unbind(); MenuInputGate.Release(this); }
}
