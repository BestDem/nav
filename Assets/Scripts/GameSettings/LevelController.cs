using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    private Screen_fader screen;

    private void Start()
    {
        screen = FindAnyObjectByType<Screen_fader>();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void TeleportOnLevel(int level)
    {
        screen.FadeIn();

        StartCoroutine(FadeInCoroutine(level));
    }

    private IEnumerator FadeInCoroutine(int level)
    {
        yield return new WaitForSecondsRealtime(1f);

        SceneManager.LoadScene(level);
    }
    
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}