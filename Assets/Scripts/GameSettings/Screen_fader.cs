using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Screen_fader : MonoBehaviour
{
    private PlayerController movementController;
    private float numIter = 10;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Image imageDark;
    private Color colorIm;

    private void Start()
    {
        movementController = FindAnyObjectByType<PlayerController>();
        colorIm = imageDark.color;

        StartCoroutine(FadeOut());

    }
    public void FadeOutPr()
    {
        StartCoroutine(FadeOut());
    }
    private IEnumerator FadeOut()
    {
        if (movementController != null)
        {
            movementController.BlockMoveAndLook(true);
        }

        while (colorIm.a > 0f)
        {
            colorIm.a -= 1f / numIter;
            imageDark.color = colorIm;
            yield return new WaitForSecondsRealtime(fadeTime / numIter);
        }

        if (movementController  != null)
        {
            movementController.BlockMoveAndLook(false);
        }


        colorIm.a = 0f;
        imageDark.color = colorIm;
    }
    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        if (movementController  != null)
        {
            movementController.BlockMoveAndLook(true);
        }

        while (colorIm.a < 1f)
        {
            colorIm.a += 1f / numIter;
            imageDark.color = colorIm;
            yield return new WaitForSecondsRealtime(fadeTime / numIter);
        }

        colorIm.a = 1f;
        imageDark.color = colorIm;
    }
}
