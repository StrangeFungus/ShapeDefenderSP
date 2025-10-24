using System;
using System.Collections.Generic;
using UnityEngine;

// We can register any kind of interfact without needing new variables and
// we can request and resolve the call for any interface
// as long as said interface has initialize its own interface on Awake().
// This static class acts like a global registry or "service locator" for interfaces.
public static class InterfaceContainer
{
    private static readonly Dictionary<Type, object> interfaces = new Dictionary<Type, object>();

    public static void Register<T>(T instance) where T : class
    {
        var type = typeof(T);

        if (instance == null)
        {
            Debug.LogWarning($"[InterfaceContainer] Tried to register null for {type.Name}.");
            return;
        }

        if (!interfaces.ContainsKey(type))
        {
            interfaces[type] = instance;
            return;
        }

        if (interfaces[type] == null)
        {
            interfaces[type] = instance;
            return;
        }

        Debug.Log($"[InterfaceContainer] {type.Name} already registered. Ignoring duplicate.");
    }

    public static T Request<T>() where T : class
    {
        if (interfaces.TryGetValue(typeof(T), out var obj))
            return obj as T;
        else
            return null;
    }
}

