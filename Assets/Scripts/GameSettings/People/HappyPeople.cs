using UnityEngine;
using UnityEngine.UI;

public class HappyPeople : IPeople
{
    private Image imageTimer;
    private void Start()
    {
        imageTimer = gameObject.GetComponentInChildren<Image>();
        imageTimer.fillAmount = 0;
        isEvil = false;

        int randomNumber = Random.Range(0, 10);
        if(randomNumber < 1)
            haveTicket = false;  //у половины счастливых людей нет билета
        else
            haveTicket = true;  //у всех счастливых людей есть билет
    }
    public override void Use()
    {
        base.Use();

        Debug.Log("Hello, I am a happy person.");
    }
}
