using System.Collections.Generic;
using UnityEngine;

// Each menu owns its lock. Closing settings must not release the scenario lock.
public static class MenuInputGate
{
    private static readonly HashSet<Object> owners = new();
    public static bool IsBlocked
    {
        get { owners.RemoveWhere(owner => owner == null); return owners.Count > 0; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { owners.Clear(); }

    public static void Acquire(Object owner) { owners.Add(owner); ApplyCursor(); }
    public static void Release(Object owner) { owners.Remove(owner); ApplyCursor(); }
    public static void ApplyCursor()
    {
        Cursor.lockState = IsBlocked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsBlocked;
    }
}
