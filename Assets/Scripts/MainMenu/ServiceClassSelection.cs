using UnityEngine;

// Shared by the menu and the common gameplay scene. API integration can read
// the selected code when requesting the class-specific scenario stack.
public static class ServiceClassSelection
{
    public static string Code { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Code = null;

    public static void Select(string code) => Code = code;
}
