using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public sealed class MobileGameControls : MonoBehaviour
{
    public static MobileGameControls Instance { get; private set; }
    public static bool Visible => Instance != null && Instance.shown;
    private bool shown = true;
    private RectTransform safeArea;
    private GameObject controls;
    private Button toggle, settings;
    private TMP_Text toggleLabel;
    private readonly List<MobileHoldButton> movement = new();
    private int? lookFinger;
    private Vector2 lookDelta;
    private readonly List<RaycastResult> uiHits = new();
    private InvenoryController interaction;
    private PauseUI pause;
    private bool jumpRequested;

    public Vector2 Movement
    {
        get
        {
            if (!shown || MenuInputGate.IsBlocked) return Vector2.zero;
            Vector2 value = Vector2.zero;
            foreach (var button in movement) if (button.Held) value += button.Direction;
            return Vector2.ClampMagnitude(value, 1);
        }
    }
    public Vector2 ConsumeLook()
    {
        var delta = lookDelta;
        lookDelta = Vector2.zero;
        return !MenuInputGate.IsBlocked ? delta : Vector2.zero;
    }
    public bool ConsumeJump()
    {
        bool value = jumpRequested && shown && !MenuInputGate.IsBlocked;
        jumpRequested = false;
        return value;
    }

    private void Awake()
    {
        Instance = this;
        interaction = GetComponent<InvenoryController>();
        pause = FindAnyObjectByType<PauseUI>();
        var canvas = RuntimeGameUI.Canvas("Game touch controls", transform, 50);
        safeArea = RuntimeGameUI.Rect("Safe area", canvas.transform, Vector2.zero, Vector2.one);
        toggle = RuntimeGameUI.Button(safeArea, "", new Vector2(.015f, .88f), new Vector2(.3f, .98f));
        toggleLabel = toggle.GetComponentInChildren<TMP_Text>();
        toggle.onClick.AddListener(Toggle);
        settings = RuntimeGameUI.Button(safeArea, "Настройки", new Vector2(.81f, .88f), new Vector2(.985f, .98f));
        settings.onClick.AddListener(() => { ResetInput(); if (pause != null) pause.GetInput(); });
        controls = RuntimeGameUI.Rect("Movement and interaction", safeArea, Vector2.zero, Vector2.one).gameObject;
        Hold("Вперёд", new Vector2(.10f, .25f), new Vector2(.20f, .39f), Vector2.up);
        Hold("Назад", new Vector2(.10f, .04f), new Vector2(.20f, .18f), Vector2.down);
        Hold("Влево", new Vector2(.015f, .145f), new Vector2(.115f, .285f), Vector2.left);
        Hold("Вправо", new Vector2(.205f, .145f), new Vector2(.305f, .285f), Vector2.right);
        RuntimeGameUI.Button(controls.transform, "Поговорить", new Vector2(.73f, .06f), new Vector2(.985f, .2f))
            .onClick.AddListener(() => { if (interaction != null) interaction.Interact(); });
        RuntimeGameUI.Button(controls.transform, "Прыжок", new Vector2(.53f, .06f), new Vector2(.71f, .2f))
            .onClick.AddListener(() => { if (!MenuInputGate.IsBlocked) jumpRequested = true; });
        Refresh();
    }
    private void Hold(string label, Vector2 min, Vector2 max, Vector2 direction)
    {
        var button = RuntimeGameUI.Button(controls.transform, label, min, max).gameObject.AddComponent<MobileHoldButton>();
        button.Direction = direction;
        movement.Add(button);
    }
    private void Update()
    {
        var area = Screen.safeArea;
        safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
        safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
        // Desktop uses M while the cursor is locked; touch keeps the on-screen button.
        if (Input.GetKeyDown(KeyCode.M) && !MenuInputGate.IsBlocked) Toggle();
        if (MenuInputGate.IsBlocked) ResetInput();
        else ReadTouchLook();
        Refresh();
    }
    private void Toggle() { shown = !shown; ResetInput(); Refresh(); MenuInputGate.ApplyCursor(); }
    private void Refresh()
    {
        bool playing = !StationController.IsTripEnded;
        toggle.gameObject.SetActive(playing && !DialogueUI.IsScenarioVisible);
        bool touchDevice = Application.isMobilePlatform || Input.touchSupported;
        toggleLabel.text = (touchDevice ? "" : "M · ") + "Показать интерфейс: " + (shown ? "вкл" : "выкл");
        controls.SetActive(playing && shown && !MenuInputGate.IsBlocked);
        settings.gameObject.SetActive(playing && shown && !DialogueUI.IsScenarioVisible);

    }
    private void ReadTouchLook()
    {
        // Only a gesture that starts outside UI can control the camera.
        // Keep its finger ID so movement and looking work simultaneously.
        bool found = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            if (!lookFinger.HasValue && touch.phase == TouchPhase.Began)
            {
                uiHits.Clear();
                if (EventSystem.current != null)
                {
                    var pointer = new PointerEventData(EventSystem.current) { position = touch.position };
                    EventSystem.current.RaycastAll(pointer, uiHits);
                }
                if (uiHits.Count == 0) lookFinger = touch.fingerId;
            }
            if (lookFinger != touch.fingerId) continue;
            found = true;
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                lookFinger = null;
            else if (touch.phase == TouchPhase.Moved)
                lookDelta += touch.deltaPosition * (180f / Mathf.Max(1, Screen.height));
        }
        if (!found) lookFinger = null;
    }
    private void ResetInput()
    {
        foreach (var button in movement) button.ResetInput();
        lookFinger = null;
        lookDelta = Vector2.zero;
        jumpRequested = false;
    }
    private void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
    private void OnDisable() { ResetInput(); }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}

public sealed class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private readonly HashSet<int> pointers = new();
    public Vector2 Direction;
    public bool Held => pointers.Count > 0;
    public void OnPointerDown(PointerEventData data) { pointers.Add(data.pointerId); }
    public void OnPointerUp(PointerEventData data) { pointers.Remove(data.pointerId); }
    public void OnPointerExit(PointerEventData data) { pointers.Remove(data.pointerId); }
    public void ResetInput() { pointers.Clear(); }
    private void OnDisable() { ResetInput(); }
}
