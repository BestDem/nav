using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text passengerText;
    [SerializeField] private TMP_Text emotionText;
    [SerializeField] private TMP_InputField input;

    private PassengerDialogue passenger;

    public void Open(PassengerDialogue target)
    {
        if (passenger != null)
        {
            passenger.OnPassengerMessage -= ShowMessage;
            passenger.OnDialogueFinished -= Close;
        }

        passenger = target;

        passenger.OnPassengerMessage += ShowMessage;
        passenger.OnDialogueFinished += Close;

        canvas.SetActive(true);
        passenger.OpenDialogue();
    }

    public void SendButton()
    {
        if (passenger == null)
            return;

        passenger.SendPlayerAnswer(input.text);
        input.text = "";
    }

    private void ShowMessage(string text, string emotion)
    {
        passengerText.text = text;
        emotionText.text = emotion;
    }

    private void Close(GameAssessmentData result)
    {
        canvas.SetActive(false);
    }
}
