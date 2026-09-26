using UnityEngine;

public class PeopleDIalogue : IPeople
{
    private void Start()
    {
        isEvil = true;
        haveTicket = true;  //у всех недовольных людей есть билет
    }
    public override void Use()
    {
        base.Use();

        var dialogue = GetComponent<PassengerDialogue>();
        if (dialogue == null) dialogue = gameObject.AddComponent<PassengerDialogue>();
        VSMDialogueWindow.Open(dialogue);
    }
}
