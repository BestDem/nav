using TMPro;
using UnityEngine;

public class DialogueResultsUI : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text score;
    [SerializeField] private TMP_Text safety;
    [SerializeField] private TMP_Text loyalty;
    [SerializeField] private TMP_Text outcome;
    [SerializeField] private TMP_Text summary;
    [SerializeField] private TMP_Text strengths;
    [SerializeField] private TMP_Text mistakes;

    public void Subscribe(PassengerDialogue passenger)
    {
        passenger.OnDialogueFinished -= Show;
        passenger.OnDialogueFinished += Show;
    }

    private void Show(GameAssessmentData data)
    {
        var r = data.result;

        canvas.SetActive(true);

        score.text = r.score.ToString();
        safety.text = r.safetyScore.ToString();
        loyalty.text = r.loyaltyScore.ToString();
        outcome.text = r.outcome;
        summary.text = r.summary;
        strengths.text = string.Join("\n• ", r.strengths ?? new string[0]);
        mistakes.text = string.Join("\n• ", r.mistakes ?? new string[0]);
    }

    public void Close()
    {
        canvas.SetActive(false);
    }
}
