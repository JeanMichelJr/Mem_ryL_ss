using System;
using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtension
{
    public static Coroutine NextFrame(this MonoBehaviour origin, Action action)
    {
        if (action == null)
        {
            return null;
        }

        return origin.StartCoroutine(InternalNextFrame(action));
    }

    private static IEnumerator InternalNextFrame(Action action)
    {
        yield return null;

        action();
    }
}
