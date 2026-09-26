using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject canvasPause;
    private bool isPause;
    private float previousTimeScale = 1;

    private void Start()
    {
        if (canvasPause != null) canvasPause.SetActive(false);
    }
    private void Update()
    {
        // Escape belongs to the scenario while it is open, not to settings.
        if (Input.GetKeyDown(KeyCode.Escape) && !DialogueUI.ConsumesEscapeThisFrame)
            GetInput();
    }

    public void GetInput()
    {
        if (DialogueUI.IsScenarioVisible) return;
        if (isPause) Close();
        else
        {
            isPause = true;
            previousTimeScale = Time.timeScale;
            MenuInputGate.Acquire(this);
            if (canvasPause != null) canvasPause.SetActive(true);
            Time.timeScale = 0;
        }
    }

    private void Close()
    {
        if (!isPause) return;
        isPause = false;
        if (canvasPause != null) canvasPause.SetActive(false);
        Time.timeScale = previousTimeScale;
        MenuInputGate.Release(this);
    }
    private void OnDisable() { Close(); }
}
