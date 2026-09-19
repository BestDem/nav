using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class SpawnPeople : MonoBehaviour
{
    [SerializeField] private List<Transform> pointsList = new List<Transform>();
    [SerializeField] private List<Transform> bisyPointsList = new List<Transform>();
    [SerializeField] private BaseTimer baseTimer;
    [SerializeField] private GameObject prefPeople;
    private int currentCountPeople = 0;
    private int maxPopleInVagon;
    private void Start()
    {
        maxPopleInVagon = pointsList.Count;
        SpawnPeopleEvent();
    }
    private void OnEnable()
    {
        StationController.ActionNextStation += RemovePopleOnStation;
    }
    private void OnDisable()
    {
        StationController.ActionNextStation -= RemovePopleOnStation;
    }

    private void SpawnPeopleEvent()
    {
        int countP = Random.Range(baseTimer.MinAddPeopleOnStation, baseTimer.MaxAddPeopleOnStation);
        if(currentCountPeople + countP > maxPopleInVagon) return;
        Debug.Log("Добавилось " + countP + " людей");

        for (int i = 0; i < countP; i++)
        {
            int idFreePoint = Random.Range(0, pointsList.Count);

            Transform ob = pointsList[idFreePoint];

            bisyPointsList.Add(ob);
            pointsList.RemoveAt(idFreePoint);

            GameObject spP = Instantiate(prefPeople, ob);
            ControllerPeople.singltonePeople.AddPeople(spP);
            currentCountPeople += 1;
        }
    }

    private void RemovePopleOnStation()
    {
        int countP = Random.Range(baseTimer.MinRemovePeopleOnStation, baseTimer.MaxRemovePeopleOnStation);
        if(currentCountPeople - countP < 0) return;
        Debug.Log("Ушли " + countP + " людей");

        for (int i = 0; i < countP; i++)
        {
            int idFreePoint = Random.Range(0, bisyPointsList.Count);

            Transform ob = bisyPointsList[idFreePoint];

            pointsList.Add(ob);
            bisyPointsList.RemoveAt(idFreePoint);
            
            ControllerPeople.singltonePeople.RemovePeople(ob.GetChild(0).gameObject);
            
            for(int j = ob.transform.childCount - 1; j >= 0; j--)
                Destroy(ob.transform.GetChild((j)).gameObject);

            currentCountPeople -= 1;
        }

        SpawnPeopleEvent();
    }
}
