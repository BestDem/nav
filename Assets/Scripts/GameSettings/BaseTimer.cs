using UnityEngine;
using UnityEngine.Rendering.Universal;
[CreateAssetMenu(fileName = "BaseTimer", menuName ="Timer")]
public class BaseTimer : ScriptableObject
{
    [SerializeField] private float timeOnStation;
    [SerializeField] private int countStation;
    [SerializeField] private int maxAddPeopleOnStation;
    [SerializeField] private int minAddPeopleOnStation;
    [SerializeField] private int maxRemovePeopleOnStation;
    [SerializeField] private int minRemovePeopleOnStation;
    [SerializeField] private int maxEvilPeople;
    [SerializeField] private int minEvilPeople;

    public float TimeOnStation => timeOnStation;
    public float CountStation => countStation;
    public int MaxAddPeopleOnStation => maxAddPeopleOnStation;
    public int MinAddPeopleOnStation => minAddPeopleOnStation;
    public int MaxRemovePeopleOnStation => maxRemovePeopleOnStation;
    public int MinRemovePeopleOnStation => minRemovePeopleOnStation;
    public int MaxEvilPeople => maxEvilPeople;
    public int MinEvilPeople => minEvilPeople;
}
