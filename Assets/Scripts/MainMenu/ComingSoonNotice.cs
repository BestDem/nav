using System.Collections;
using UnityEngine;

public class ComingSoonNotice : MonoBehaviour
{
    [SerializeField] private GameObject message;
    [SerializeField] private float duration = 3f;

    private Coroutine hideCoroutine;

    private void OnEnable()
    {
        message.SetActive(false);
    }

    public void Show()
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        message.SetActive(true);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(duration);

        message.SetActive(false);
        hideCoroutine = null;
    }

    private void OnDisable()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (message != null)
            message.SetActive(false);
    }
}