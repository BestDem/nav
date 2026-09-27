using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ServiceClassMenu : MonoBehaviour
{
    [SerializeField] private RectTransform shade;
    [SerializeField] private GameObject title;
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup selection;
    [SerializeField] private Button[] cards;
    [SerializeField] private LevelController levelController;
    [SerializeField] private string[] serviceClasses;
    [SerializeField] private float transitionDuration = 0.45f;

    private Vector2 collapsedAnchor;
    private bool transitioning;
    private bool launching;

    private void OnEnable() { MenuInputGate.Acquire(this); }
    private void OnDisable() { MenuInputGate.Release(this); }

    private void Awake()
    {
        collapsedAnchor = shade.anchorMax;
        selection.gameObject.SetActive(false);
    }

    public void ShowClasses()
    {
        if (transitioning || launching || selection.gameObject.activeSelf) return;
        StartCoroutine(Transition(true));
    }

    public void ShowWelcome()
    {
        if (transitioning || launching) return;
        StartCoroutine(Transition(false));
    }

    private IEnumerator Transition(bool opening)
    {
        transitioning = true;
        title.SetActive(false);
        startButton.gameObject.SetActive(false);
        selection.interactable = false;
        selection.blocksRaycasts = false;
        selection.gameObject.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(null);

        Vector2 from = shade.anchorMax;
        Vector2 to = opening ? Vector2.one : collapsedAnchor;
        float duration = Mathf.Max(0.01f, transitionDuration);
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            shade.anchorMax = Vector2.Lerp(from, to, t);
            selection.alpha = opening ? Mathf.Clamp01((t - 0.4f) / 0.6f) : 1 - t;
            yield return null;
        }

        shade.anchorMax = to;
        selection.alpha = opening ? 1 : 0;
        selection.gameObject.SetActive(opening);
        selection.interactable = opening;
        selection.blocksRaycasts = opening;
        title.SetActive(!opening);
        startButton.gameObject.SetActive(!opening);
        transitioning = false;
        EventSystem.current?.SetSelectedGameObject(opening ? cards[0].gameObject : startButton.gameObject);
    }

    public void SelectClass(int index)
    {
        if (transitioning || launching || index < 0 || index >= serviceClasses.Length) return;
        ServiceClassSelection.Select(serviceClasses[index]);
        VSMGameSession.Instance?.PrepareTrip();
        launching = true;
        selection.interactable = false;
        levelController.TeleportOnLevel(1);
    }
}
