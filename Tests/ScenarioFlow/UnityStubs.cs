// Minimal deterministic host for testing production scenario logic without launching Unity.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
namespace UnityEngine
{
    public class SerializeField : Attribute { }
    public class Object
    {
        public static void Destroy(Object value) { if (value is MonoBehaviour b) b.gameObject?.Remove(b); }
        public static void DontDestroyOnLoad(Object value) { }
    }
    public class GameObject : Object
    {
        private readonly List<MonoBehaviour> components = new();
        public T AddComponent<T>() where T : MonoBehaviour, new()
        {
            var value = new T { gameObject = this }; components.Add(value);
            TestHost.Call(value, "Awake"); return value;
        }
        public T GetComponent<T>() where T : class => components.Find(c => c is T) as T;
        public void Remove(MonoBehaviour value) => components.Remove(value);
    }
    public class Transform : Object { }
    public static class Time { public static float timeScale = 1; public static float deltaTime = .016f; }
    public class MonoBehaviour : Object
    {
        public GameObject gameObject;
        public bool enabled = true;
        public Transform transform = new();
        public void CancelInvoke() { }
        public void Invoke(string name, float delay) { }
        public object StartCoroutine(IEnumerator iterator) { while (iterator.MoveNext()) { } return null; }
    }
    public static class Random { public static int Range(int a, int b) => a; }
    public static class Mathf { public static int Max(int a, int b) => Math.Max(a,b); public static float Clamp01(float v) => Math.Clamp(v,0,1); }
    public static class Debug { public static void LogError(object value) { } public static void Log(object value, Object context = null) { } }
    public static class JsonUtility
    {
        private static readonly JsonSerializerOptions options = new() { IncludeFields = true };
        public static T FromJson<T>(string text) => JsonSerializer.Deserialize<T>(text, options);
        public static string ToJson(object value) => JsonSerializer.Serialize(value, options);
    }
}
public class BaseTimer { public int MaxEvilPeople => 3; public float TimeOnStation => 30; }
public class HappyPeople : UnityEngine.MonoBehaviour { }
public class PeopleDIalogue : UnityEngine.MonoBehaviour { }
public static class TestHost
{
    public static void Call(object target, string method) => target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, null);
}
public class VSMApiClient
{
    public static VSMApiClient Instance = new();
    public bool HasToken = true;
    public readonly Queue<(string Method, string Path, object Reply, bool Fail)> Responses = new();
    public readonly List<string> Requests = new();
    public void Expect(string method, string path, object reply, bool fail = false) => Responses.Enqueue((method,path,reply,fail));
    public IEnumerator Get(string path, Action<string> ok, Action<long,string> fail) => Request("GET",path,ok,fail);
    public IEnumerator PostJson(string path, string body, Action<string> ok, Action<long,string> fail) => Request("POST",path,ok,fail);
    private IEnumerator Request(string method, string path, Action<string> ok, Action<long,string> fail)
    {
        Requests.Add(method+" "+path);
        if (Responses.Count == 0) throw new Exception("Unexpected request: "+Requests[^1]);
        var next = Responses.Dequeue();
        if (next.Method != method || next.Path != path) throw new Exception("Unexpected endpoint: "+Requests[^1]);
        if (next.Fail) fail(503, "Test interruption"); else ok(UnityEngine.JsonUtility.ToJson(next.Reply));
        yield break;
    }
}

public class Screen_fader { public void FadeIn() { } public void FadeOutPr() { } }
public static class DialogueUI
{
    public static bool IsScenarioVisible;
    public static string CurrentAssessmentText = "Последняя оценка";
    public static void HideForTripCompletion() { IsScenarioVisible = false; }
}
public static class MenuInputGate
{
    public static bool IsBlocked;
    public static void Acquire(object owner) { IsBlocked = true; }
    public static void Release(object owner) { IsBlocked = false; }
}
public static class TripCompletionView
{
    public static int Shows;
    public static string Assessment;
    public static void Show(UnityEngine.Transform owner, string text) { Shows++; Assessment = text; }
}
