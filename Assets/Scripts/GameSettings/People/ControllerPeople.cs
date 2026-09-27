using System.Collections.Generic;
using UnityEngine;

public class ControllerPeople : MonoBehaviour
{
    static public ControllerPeople singltonePeople {get; private set;}
    [SerializeField] private List<GameObject> peopleEvil = new List<GameObject>();
    [SerializeField] private List<GameObject> peopleHappy = new List<GameObject>();
    [SerializeField] private BaseTimer baseT;
    private void Awake()
    {
        if(singltonePeople == null)
        {
            singltonePeople = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Удален дубликат контроллера людей");
        }
    }
    private int countEvelPeopleInVagon = 0;

    public void AddPeople(GameObject people)
    {
        if(countEvelPeopleInVagon < baseT.MinEvilPeople)
        {
            AddEvilPeople(people);
        }
        else if(countEvelPeopleInVagon < baseT.MaxEvilPeople)
        {
            int r = Random.Range(0, 2);
            if(r < 1)
                AddEvilPeople(people);
            else
                AddHappyPeople(people);
        }
        else
        {
            AddHappyPeople(people);
        }
    }

    public void RemovePeople(GameObject people)
    {
        if(people.TryGetComponent(out IPeople peopleScript))
        {
            if(peopleScript.GetComponent<HappyPeople>() == null)
            {
                peopleEvil.Remove(people);
                countEvelPeopleInVagon -= 1;
            }
            else
            {
                peopleHappy.Remove(people);
            }
        }
    }

    private void AddEvilPeople(GameObject people)
    {
        int r = 0;

        switch (r)
        {
            case 0:
                people.AddComponent<PeopleDIalogue>();
                break;

            //case 1:
            //    people.AddComponent<FeelBadPeople>();
            //    break;
        }

        peopleEvil.Add(people);
        countEvelPeopleInVagon += 1;
    }
    private void AddHappyPeople(GameObject people)
    {
        people.AddComponent<HappyPeople>();
        peopleHappy.Add(people);
    }
}
