using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PeopleDIalogue : IPeople
{
    private Image imageTimer;
    private float currentTimer;
    private float maxTimer = 30;
    private bool isTalk = false;
    public void IsTalk(bool talk) => isTalk = talk;
    private void Start()
    {
        currentTimer = maxTimer;
        isEvil = true;
        haveTicket = true;  //у всех недовольных людей есть билет
        imageTimer = gameObject.GetComponentInChildren<Image>();
    }
    private void Update()
    {
        if(isTalk) return;

        if(isEvil == false)
        {
            imageTimer.fillAmount = 0;
        }
        currentTimer -= 0.2f * Time.deltaTime;
        currentTimer = Mathf.Clamp(currentTimer, 0, maxTimer);
        imageTimer.fillAmount = currentTimer / maxTimer;
    }
    public override void Use()
    {
        base.Use();

        var dialogue = GetComponent<PassengerDialogue>();
        if (dialogue == null) dialogue = gameObject.AddComponent<PassengerDialogue>();
        VSMDialogueWindow.Open(dialogue);
    }
}
