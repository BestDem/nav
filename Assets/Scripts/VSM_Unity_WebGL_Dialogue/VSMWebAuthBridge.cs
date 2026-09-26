using UnityEngine;

public class VSMWebAuthBridge : MonoBehaviour
{
    public static VSMWebAuthBridge Instance { get; private set; }

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
        VSMApiClient.Instance.SetAccessToken(token);

        if (VSMGameSession.Instance != null)
            VSMGameSession.Instance.OnWebAuthenticationReady();
    }
}
