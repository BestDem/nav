using UnityEngine;

public abstract class IPeople : MonoBehaviour, InteractObject
{
    protected bool haveTicket;
    protected bool isEvil = true;
    public bool HaveTicket { get => haveTicket; set => haveTicket = value; }
    //public abstract void Use();

    public virtual void Use()
    {
        if(isEvil == false) return;
    }

    public virtual void StateHappy()
    {
        isEvil = false;
    }

}
