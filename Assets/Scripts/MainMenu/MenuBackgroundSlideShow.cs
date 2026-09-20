using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuBackgroundSlideShow : MonoBehaviour
{
    [SerializeField] private Sprite[] backgrounds;
    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;

    [SerializeField, Min(0f)] private float displayDuration = 8f;
    [SerializeField, Min(0.1f)] private float transitionDuration = 2f;

    private int currentIndex;

    private void OnEnable()
    {
        if (currentImage == null || nextImage == null ||
            currentImage == nextImage ||
            backgrounds == null || backgrounds.Length == 0)
        {
            Debug.LogWarning("Проверь картинки и ссылки на слои фона.", this);
            return;
        }

        foreach (Sprite background in backgrounds)
        {
            if (background == null)
            {
                Debug.LogWarning("В списке фонов есть пустой элемент.", this);
                return;
            }
        }

        currentIndex = 0;
        currentImage.sprite = backgrounds[currentIndex];

        currentImage.color = Color.white;
        nextImage.color = new Color(1f, 1f, 1f, 0f);

        if (backgrounds.Length > 1)
        {
            StartCoroutine(PlaySlideShow());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (nextImage != null)
        {
            SetAlpha(nextImage, 0f);
        }
    }

    private IEnumerator PlaySlideShow()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, displayDuration));

            int nextIndex = (currentIndex + 1) % backgrounds.Length;
            nextImage.sprite = backgrounds[nextIndex];
            SetAlpha(nextImage, 0f);

            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, transitionDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / duration);
                float alpha = Mathf.SmoothStep(0f, 1f, progress);

                SetAlpha(nextImage, alpha);

                yield return null;
            }

            currentImage.sprite = nextImage.sprite;
            SetAlpha(nextImage, 0f);

            currentIndex = nextIndex;
        }
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
