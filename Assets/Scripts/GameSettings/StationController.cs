using System;
using System.Threading;
using UnityEngine;

public class StationController : MonoBehaviour
{
    static public StationController singltoneStationContr {get; private set;}
    [SerializeField] private BaseTimer baseTimer;
    public static event Action ActionNextStation;
    public static event Action ActionEndGame;
    [SerializeField] private Screen_fader screen_Fader;
    private float time = 0;
    private int currentStation = 1;
    private void Start()
    {
        if(singltoneStationContr == null)
        {
            singltoneStationContr = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Удален дубликат контроллера");
        }
        time = baseTimer.TimeOnStation;
    }
    private void Update()
    {
        if(time > 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            ActionNextStation?.Invoke();
            screen_Fader.FadeIn();
            time = 10;
            Invoke("NextStationFade", 1);

            if(currentStation == baseTimer.CountStation)
            {
                Debug.Log("Игра завершена");
                ActionEndGame?.Invoke();
            }
        }
    }

    private void NextStationFade()
    {
        screen_Fader.FadeOutPr();
        currentStation += 1;
        time = baseTimer.TimeOnStation;

        Debug.Log("Следующая станция номер: " + currentStation);
    }
}
