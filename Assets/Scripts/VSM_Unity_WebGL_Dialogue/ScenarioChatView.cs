using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Code-built uGUI layout keeps the MVP independent of scene/prefab references.
public sealed class ScenarioChatView
{
    public GameObject Screen { get; private set; }
    public TMP_InputField Input { get; private set; }
    public Button Send, Voice, Finish, Hide, Retry, Next;
    public TMP_Text Status, FinishLabel;
    private ScrollRect scroll;
    private RectTransform content;
    private TMP_FontAsset font;
    private Sprite rounded;
    private readonly Color blue = new Color32(49, 166, 249, 255);
    private readonly Color pale = new Color32(199, 231, 252, 255);
    private readonly Color ink = new Color32(35, 57, 76, 255);

    public ScenarioChatView(Transform parent, TMP_FontAsset sourceFont)
    {
        font = sourceFont != null ? sourceFont : TMP_Settings.defaultFontAsset;
        rounded = MakeRoundSprite();
        Screen = Rect("Scenario screen", parent, Vector2.zero, Vector2.one).gameObject;
        Finish = ButtonAt("Завершить", Screen.transform, new Vector2(.055f, .905f), new Vector2(.16f, .955f), pale, ink);
        FinishLabel = Finish.GetComponentInChildren<TMP_Text>();
        Hide = ButtonAt("Свернуть · Esc", Screen.transform, new Vector2(.18f, .905f), new Vector2(.315f, .955f), Color.white, ink);
        var panel = Panel("Conversation", Screen.transform, new Vector2(.68f, .06f), new Vector2(.955f, .955f), pale);
        var caption = Label("Conversation heading", panel, "ПЕРЕПИСКА", 15, ink);
        Place(caption.rectTransform, new Vector2(.06f, .93f), new Vector2(.94f, .985f));
        var viewport = Rect("Viewport", panel, new Vector2(.035f, .025f), new Vector2(.965f, .93f));
        viewport.gameObject.AddComponent<RectMask2D>();
        scroll = panel.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28;
        content = Rect("Messages", viewport, new Vector2(0, 1), Vector2.one);
        content.pivot = new Vector2(.5f, 1);
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20;
        layout.padding = new RectOffset(6, 6, 8, 14);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fit = content.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content;

        Status = Label("Status", Screen.transform, "Подключение к серверу…", 18, ink);
        Place(Status.rectTransform, new Vector2(.055f, .16f), new Vector2(.65f, .26f));
        Retry = ButtonAt("Повторить", Screen.transform, new Vector2(.055f, .29f), new Vector2(.18f, .34f), Color.white, ink);
        Next = ButtonAt("Следующий сценарий", Screen.transform, new Vector2(.055f, .36f), new Vector2(.26f, .415f), pale, ink);
        Retry.gameObject.SetActive(false);
        Next.gameObject.SetActive(false);

        var composer = Panel("Composer", Screen.transform, new Vector2(.055f, .06f), new Vector2(.65f, .135f), pale);
        var field = Rect("Message input", composer, new Vector2(.025f, .08f), new Vector2(.73f, .92f));
        Input = field.gameObject.AddComponent<TMP_InputField>();
        var inputBg = field.gameObject.AddComponent<Image>();
        inputBg.color = Color.clear;
        Input.targetGraphic = inputBg;
        var inputViewport = Rect("Input viewport", field, Vector2.zero, Vector2.one);
        inputViewport.gameObject.AddComponent<RectMask2D>();
        var inputText = Label("Text", inputViewport, "", 21, ink);
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        var placeholder = Label("Placeholder", inputViewport, "Введите ответ…", 21, new Color32(105, 157, 186, 255));
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        Input.textViewport = inputViewport;
        Input.textComponent = inputText;
        Input.placeholder = placeholder;
        Input.fontAsset = font;
        Input.pointSize = 21;
        Input.lineType = TMP_InputField.LineType.SingleLine;
        Input.characterLimit = 4000;
        Input.richText = false;
        Voice = ButtonAt("", composer, new Vector2(.75f, .12f), new Vector2(.815f, .88f), pale, blue);
        DrawMicrophone(Voice.transform);
        Send = ButtonAt("Отправить", composer, new Vector2(.825f, .12f), new Vector2(.985f, .88f), blue, Color.white);
    }

    public void ClearMessages()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i).gameObject;
            child.SetActive(false);
            Object.Destroy(child);
        }
    }

    public void AddMessage(string text, bool conductor, string suffix = "")
    {
        Canvas.ForceUpdateCanvases();
        float width = Mathf.Max(180, scroll.viewport.rect.width - 12);
        float bubbleWidth = width * .84f;
        var row = Rect("Message row", content, Vector2.zero, Vector2.one);
        var bubble = Panel(conductor ? "Conductor" : "Passenger", row,
            new Vector2(conductor ? .16f : 0, 0), new Vector2(conductor ? 1 : .84f, 1), conductor ? Color.white : blue);
        var body = Label("Message", bubble, text, 19, conductor ? ink : Color.white);
        float textHeight = body.GetPreferredValues(text, bubbleWidth - 30, Mathf.Infinity).y;
        float height = Mathf.Max(90, textHeight + 56);
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
        var author = Label("Speaker", bubble, (conductor ? "ПРОВОДНИК" : "ПАССАЖИР") + suffix, 12,
            conductor ? new Color32(100, 135, 158, 255) : new Color32(222, 243, 255, 255));
        Place(author.rectTransform, new Vector2(0, 1), Vector2.one);
        author.rectTransform.offsetMin = new Vector2(15, -32);
        author.rectTransform.offsetMax = new Vector2(-15, -10);
        body.rectTransform.offsetMin = new Vector2(15, 14);
        body.rectTransform.offsetMax = new Vector2(-15, -38);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        scroll.verticalNormalizedPosition = 0;
    }

    private RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        var r = Rect(name, parent, min, max);
        var image = r.gameObject.AddComponent<Image>();
        image.sprite = rounded;
        image.type = Image.Type.Sliced;
        image.color = color;
        return r;
    }
    private Button ButtonAt(string text, Transform parent, Vector2 min, Vector2 max, Color color, Color textColor)
    {
        var r = Panel(text.Length == 0 ? "Microphone" : text, parent, min, max, color);
        var button = r.gameObject.AddComponent<Button>();
        button.targetGraphic = r.GetComponent<Image>();
        var colors = button.colors;
        colors.highlightedColor = new Color(.88f, .95f, 1);
        colors.pressedColor = new Color(.72f, .86f, .96f);
        colors.disabledColor = new Color(.72f, .78f, .82f, .65f);
        button.colors = colors;
        var label = Label("Label", r, text, 18, textColor);
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 12;
        label.fontSizeMax = 18;
        return button;
    }
    private TMP_Text Label(string name, Transform parent, string text, int size, Color color)
    {
        var label = Rect(name, parent, Vector2.zero, Vector2.one).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.fontSize = size;
        label.color = color;
        label.text = text;
        label.richText = false;
        label.raycastTarget = false;
        return label;
    }
    private void DrawMicrophone(Transform parent)
    {
        Panel("Mic capsule", parent, new Vector2(.38f, .36f), new Vector2(.62f, .82f), blue).GetComponent<Image>().raycastTarget = false;
        Panel("Mic stem", parent, new Vector2(.47f, .17f), new Vector2(.53f, .37f), blue).GetComponent<Image>().raycastTarget = false;
        Panel("Mic base", parent, new Vector2(.34f, .14f), new Vector2(.66f, .20f), blue).GetComponent<Image>().raycastTarget = false;
    }
    private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.SetParent(parent, false);
        Place(r, min, max);
        return r;
    }
    private static void Place(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }
    private static Sprite MakeRoundSprite()
    {
        const int size = 48;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Max(Mathf.Abs(x - 23.5f) - 11.5f, 0);
            float dy = Mathf.Max(Mathf.Abs(y - 23.5f) - 11.5f, 0);
            float alpha = Mathf.Clamp01(12 - Mathf.Sqrt(dx * dx + dy * dy));
            texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(14, 14, 14, 14));
    }
}
