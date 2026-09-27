using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RuntimeGameUI
{
    public static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min; rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    public static Canvas Canvas(string name, Transform parent, int order)
    {
        var root = Rect(name, parent, Vector2.zero, Vector2.one);
        var canvas = root.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        var scaler = root.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = .5f;
        root.gameObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static TMP_Text Text(Transform parent, string value, int size)
    {
        var label = Rect("Label", parent, Vector2.zero, Vector2.one).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = value;
        label.fontSize = size;
        label.color = Color.white;
        label.richText = false;
        label.raycastTarget = false;
        label.alignment = TextAlignmentOptions.Center;
        return label;
    }

    public static Button Button(Transform parent, string value, Vector2 min, Vector2 max)
    {
        var rect = Rect(value, parent, min, max);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(.06f, .2f, .32f, .9f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var label = Text(rect, value, 22);
        label.rectTransform.offsetMin = new Vector2(8, 4);
        label.rectTransform.offsetMax = new Vector2(-8, -4);
        label.enableAutoSizing = true;
        label.fontSizeMin = 14;
        label.fontSizeMax = 22;
        return button;
    }
}
