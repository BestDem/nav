using System;
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
    private bool ended;
    public static bool IsTripEnded => singltoneStationContr != null && singltoneStationContr.ended;
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
        if (ended || VSMGameSession.Instance == null || !VSMGameSession.Instance.IsReady) return;
        // Finish on the tenth confirmed assessment, without waiting for a stop.
        if (VSMGameSession.Instance.TripComplete)
        {
            EndTrip();
            return;
        }
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
        }
    }

    private void EndTrip()
    {
        if (ended) return;
        ended = true;
        CancelInvoke();
        string assessment = DialogueUI.CurrentAssessmentText;
        DialogueUI.HideForTripCompletion();
        TripCompletionView.Show(transform, assessment);
        Time.timeScale = 0;
        MenuInputGate.Acquire(this);
        ActionEndGame?.Invoke();
    }

    private void OnDestroy()
    {
        MenuInputGate.Release(this);
        if (ended) Time.timeScale = 1;
        if (singltoneStationContr == this) singltoneStationContr = null;
    }

    private void NextStationFade()
    {
        screen_Fader.FadeOutPr();
        currentStation += 1;
        time = baseTimer.TimeOnStation;

        Debug.Log("Следующая станция номер: " + currentStation);
    }
}
