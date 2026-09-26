using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class VSMApiClient : MonoBehaviour
{
    public static VSMApiClient Instance { get; private set; }

    [SerializeField] private string baseUrl = "https://vsm-edu.ru";
    public string AccessToken { get; private set; }

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

    public void SetAccessToken(string token)
    {
        AccessToken = token;
        Debug.Log("VSM: token received from website.");
    }

    public bool HasToken => !string.IsNullOrEmpty(AccessToken);

    public IEnumerator Get(string endpoint, Action<string> ok, Action<long,string> fail = null)
    {
        if (!HasToken) { fail?.Invoke(401, "No website token."); yield break; }

        using var r = UnityWebRequest.Get(baseUrl + endpoint);
        Prepare(r);
        yield return r.SendWebRequest();
        Handle(r, ok, fail);
    }

    public IEnumerator PostJson(string endpoint, string json, Action<string> ok, Action<long,string> fail = null)
    {
        if (!HasToken) { fail?.Invoke(401, "No website token."); yield break; }

        using var r = new UnityWebRequest(baseUrl + endpoint, "POST");
        r.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json ?? ""));
        r.downloadHandler = new DownloadHandlerBuffer();
        r.SetRequestHeader("Content-Type", "application/json");
        Prepare(r);
        yield return r.SendWebRequest();
        Handle(r, ok, fail);
    }

    private void Prepare(UnityWebRequest r)
    {
        r.SetRequestHeader("Accept", "application/json");
        r.SetRequestHeader("Authorization", "Bearer " + AccessToken);
    }

    private void Handle(UnityWebRequest r, Action<string> ok, Action<long,string> fail)
    {
        string body = r.downloadHandler?.text ?? "";

        if (r.result == UnityWebRequest.Result.Success)
            ok?.Invoke(body);
        else
            fail?.Invoke(r.responseCode, body);
    }
}
