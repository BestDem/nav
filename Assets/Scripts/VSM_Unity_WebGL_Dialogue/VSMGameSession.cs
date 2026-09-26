using System;
using System.Collections.Generic;
using UnityEngine;

public class VSMGameSession : MonoBehaviour
{
    public static VSMGameSession Instance { get; private set; }

    public string SessionId { get; private set; }
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

        OnWebAuthenticationReady();
    }

    public void OnWebAuthenticationReady()
    {
        CreateOrResumeSession();
    }

    public void CreateOrResumeSession()
    {
        StartCoroutine(VSMApiClient.Instance.PostJson(
            "/api/game/sessions",
            "",
            response =>
            {
                var r = JsonUtility.FromJson<GameSessionResponse>(response);

                if (r?.data == null)
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
        Debug.LogError(message);
        OnError?.Invoke(message);
    }
}
