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

        Debug.Log("Hello, I am an evil person.");
    }
}
