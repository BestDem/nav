using UnityEngine;
using System.Runtime.InteropServices;

public class VSMWebAuthBridge : MonoBehaviour
{
    public static VSMWebAuthBridge Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var root = new GameObject("VSM_WEB_BRIDGE");
        root.AddComponent<VSMApiClient>();
        root.AddComponent<VSMGameSession>();
        root.AddComponent<VSMWebAuthBridge>();
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void VSM_RequestWebsiteToken(string objectName);
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        VSM_RequestWebsiteToken(gameObject.name);
#endif
    }

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

    // Website calls:
    // unityInstance.SendMessage("VSM_WEB_BRIDGE", "SetAccessToken", token);
    public void SetAccessToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || VSMApiClient.Instance == null) return;
        VSMApiClient.Instance.SetAccessToken(token);

        if (VSMGameSession.Instance != null)
            VSMGameSession.Instance.OnWebAuthenticationReady();
    }
}
