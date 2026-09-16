using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject canvasPause;
    private bool isPause = false;
    private PlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GetInput();
    }

    public void GetInput()
    {
        isPause = !isPause;

        if (isPause == true)
        {
            playerController.BlockMoveAndLook(true);
            canvasPause.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playerController.BlockMoveAndLook(false);
            canvasPause.SetActive(false);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

