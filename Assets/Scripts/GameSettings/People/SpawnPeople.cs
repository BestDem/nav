using System.Collections.Generic;
using UnityEngine;

public class SpawnPeople : MonoBehaviour
{
    [SerializeField] private List<Transform> pointsList = new();
    [SerializeField] private List<Transform> bisyPointsList = new();
    [SerializeField] private BaseTimer baseTimer;
    [SerializeField] private GameObject prefPeople;

    private void Start() { SpawnPeopleEvent(); }
    private void OnEnable() { StationController.ActionNextStation += RemovePopleOnStation; }
    private void OnDisable() { StationController.ActionNextStation -= RemovePopleOnStation; }

    private void SpawnPeopleEvent()
    {
        // Fill the five seats; scenario allocation is separate from crowd spawning.
        while (pointsList.Count > 0)
        {
            int index = Random.Range(0, pointsList.Count);
            var point = pointsList[index];
            pointsList.RemoveAt(index);
            bisyPointsList.Add(point);
            var person = Instantiate(prefPeople, point);
            person.transform.rotation = point.rotation;
            ControllerPeople.singltonePeople.AddPeople(person);
        }
    }

    private void RemovePopleOnStation()
    {
        for (int i = bisyPointsList.Count - 1; i >= 0; i--)
        {
            var point = bisyPointsList[i];
            if (point.childCount == 0) continue;
            var person = point.GetChild(0).gameObject;
            var dialogue = person.GetComponent<PassengerDialogue>();
            // Never destroy a passenger's in-flight request or unfinished scenario.
            if (dialogue != null && (dialogue.IsBusy || DialogueUI.IsViewing(dialogue) ||
                (dialogue.HasScenario && !dialogue.IsFinished))) continue;
            ControllerPeople.singltonePeople.RemovePeople(person);
            person.transform.SetParent(null);
            Destroy(person);
            bisyPointsList.RemoveAt(i);
            pointsList.Add(point);
        }
        SpawnPeopleEvent();
    }
}
