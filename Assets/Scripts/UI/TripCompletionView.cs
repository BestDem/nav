using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// A real overlay Canvas stays above scene UI and receives touch events while paused.
public static class TripCompletionView
{
    public static void Show(Transform owner, string lastAssessment)
    {
        var canvas = RuntimeGameUI.Canvas("Trip completion", owner, 1000);
        var background = canvas.gameObject.AddComponent<Image>();
        background.color = new Color(.025f, .065f, .11f, .98f);
        var panel = RuntimeGameUI.Rect("Results", canvas.transform, new Vector2(.06f, .05f), new Vector2(.94f, .95f));
        var heading = RuntimeGameUI.Rect("Heading", panel, new Vector2(0, .8f), Vector2.one);
        RuntimeGameUI.Text(heading, "Поездка завершена\nВсе 10 сценариев пройдены", 30);
        var viewport = RuntimeGameUI.Rect("Assessment viewport", panel, new Vector2(0, .2f), new Vector2(1, .77f));
        viewport.gameObject.AddComponent<Image>().color = new Color(.08f, .16f, .24f, 1);
        viewport.gameObject.AddComponent<RectMask2D>();
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;
        var content = RuntimeGameUI.Rect("Assessment", viewport, new Vector2(0, 1), Vector2.one);
        content.pivot = new Vector2(.5f, 1);
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 18, 18);
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var text = RuntimeGameUI.Text(content,
            string.IsNullOrEmpty(lastAssessment) ? "Спасибо за прохождение!" : "Последняя оценка\n\n" + lastAssessment, 23);
        text.alignment = TextAlignmentOptions.TopLeft;
        scroll.content = content;
        var button = RuntimeGameUI.Button(panel, "Вернуться в меню", new Vector2(.2f, .01f), new Vector2(.8f, .14f));
        button.onClick.AddListener(() => { Time.timeScale = 1; SceneManager.LoadScene(0); });
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        scroll.verticalNormalizedPosition = 1;
    }
}
