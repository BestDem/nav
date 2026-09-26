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

    private PassengerDialogue subscribed;

    private void Awake()
    {
        // Wire the existing Main/Canvas/DIalogue UI without rewriting the scene.
        if (gameObject.name == "DIalogue" && GetComponent<DialogueUI>() == null)
            gameObject.AddComponent<DialogueUI>().ConfigureStandalone();
    }

    private void OnDestroy()
    {
        if (subscribed != null) subscribed.OnDialogueFinished -= Show;
    }

    public void Subscribe(PassengerDialogue passenger)
    {
        if (subscribed != null) subscribed.OnDialogueFinished -= Show;
        subscribed = passenger;
        passenger.OnDialogueFinished += Show;
    }

    private void Show(GameAssessmentData data)
    {
        var r = data.result;

        if (canvas != null) canvas.SetActive(true);

        if (score != null) score.text = r.score.ToString();
        if (safety != null) safety.text = r.safetyScore.ToString();
        if (loyalty != null) loyalty.text = r.loyaltyScore.ToString();
        if (outcome != null) outcome.text = r.outcome;
        if (summary != null) summary.text = r.summary;
        if (strengths != null) strengths.text = string.Join("\n• ", r.strengths ?? new string[0]);
        if (mistakes != null) mistakes.text = string.Join("\n• ", r.mistakes ?? new string[0]);
    }

    public void Close()
    {
        if (canvas != null) canvas.SetActive(false);
    }
}
