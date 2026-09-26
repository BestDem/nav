using UnityEngine;


public class PeopleCastom : MonoBehaviour
{
    [SerializeField] private GameObject[] hair;
    [SerializeField] private GameObject[] Moustache;
    [SerializeField] private GameObject[] Tshirt;
    [SerializeField] private GameObject[] shirt;
    [SerializeField] private GameObject[] boots;
    [SerializeField] private GameObject[] jeans;

    [SerializeField] private bool isMale;

    private void Start()
    {
        if (isMale)
        {
            int i = Random.Range(0, 10);
            if(i < 3)
            {
                Moustache[Random.Range(0, Moustache.Length)].SetActive(true);
            }

        }

        hair[Random.Range(0, hair.Length)].SetActive(true);
        Tshirt[Random.Range(0, Tshirt.Length)].SetActive(true);
        shirt[Random.Range(0, shirt.Length)].SetActive(true);
        boots[Random.Range(0, boots.Length)].SetActive(true);
        jeans[Random.Range(0, jeans.Length)].SetActive(true);
    }
}
