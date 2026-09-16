using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private string ItemName = "ITEM";

    public Sprite GetSprite()
    {
        return sprite;
    }

    public void PlayPickupSound(int index)
    {
        //SoundController.singletonSound.Play2DSongByIndex(1);
    }

    public string GetItemName()
    {
        return ItemName;
    }
}
