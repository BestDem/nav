using System;
using System.Collections.Generic;
using UnityEngine;

public class VSMGameSession : MonoBehaviour
{
    public static VSMGameSession Instance { get; private set; }

    public string SessionId { get; private set; }
    public bool IsReady { get; private set; }
    private bool loading;
    public bool IsLoading => loading;
    public string LastError { get; private set; }
    private readonly List<GameScenario> available = new();

    public IReadOnlyList<GameScenario> Available => available;

    public event Action OnSessionReady;
    public event Action<string> OnError;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);


    }

    private void Start()
    {
        if (VSMApiClient.Instance != null && VSMApiClient.Instance.HasToken)
            CreateOrResumeSession();
    }

    public void OnWebAuthenticationReady()
    {
        CreateOrResumeSession();
    }

    public void CreateOrResumeSession()
    {
        if (loading || IsReady || VSMApiClient.Instance == null || !VSMApiClient.Instance.HasToken)
            return;
        LastError = null;
        loading = true;
        if (!string.IsNullOrEmpty(SessionId)) { LoadScenarios(); return; }
        StartCoroutine(VSMApiClient.Instance.PostJson(
            "/api/game/sessions",
            "",
            response =>
            {
                var r = JsonUtility.FromJson<GameSessionResponse>(response);

                if (r?.data == null || string.IsNullOrEmpty(r.data.id))
                {
                    Error("Не удалось создать игровую сессию.");
                    return;
                }

                SessionId = r.data.id;
                LoadScenarios();
            },
            ApiError
        ));
    }

    private void LoadScenarios()
    {
        StartCoroutine(VSMApiClient.Instance.Get(
            $"/api/game/sessions/{SessionId}/scenarios",
            response =>
            {
                var r = JsonUtility.FromJson<GameScenarioListResponse>(response);

                if (r?.data?.scenarios == null)
                {
                    Error("Сервер не вернул сценарии.");
                    return;
                }

                available.Clear();

                foreach (var scenario in r.data.scenarios)
                    if (scenario != null && scenario.status == "pending")
                        available.Add(scenario);

                loading = false;
                IsReady = true;
                OnSessionReady?.Invoke();
            },
            ApiError
        ));
    }

    // Берёт случайный код и удаляет его из локального списка.
    public GameScenario TakeRandomScenario()
    {
        if (available.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, available.Count);
        GameScenario result = available[index];
        available.RemoveAt(index);

        return result;
    }

    private void ApiError(long code, string body)
    {
        Error($"API {code}: {body}");
    }

    private void Error(string message)
    {
        loading = false;
        LastError = message;
        Debug.LogError(message);
        OnError?.Invoke(message);
    }
}
