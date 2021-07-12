using System;

namespace Cobalt
{
    public static class Log
    {
        #if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            public static void Info(object tag, string message) { UnityEngine.Debug.Log($"[{tag.GetType().Name}] {message}"); }
            public static void Warning(object tag, string message) { UnityEngine.Debug.LogWarning($"[{tag.GetType().Name}] {message}"); }
            public static void Error(object tag, Exception e) { UnityEngine.Debug.LogError(e); }
        #else
            public static void Info(object tag, string message) { Console.WriteLine($"[{tag.GetType().Name}] {message}"); }
            public static void Warning(object tag, string message) { Console.WriteLine($"[{tag.GetType().Name}] {message}"); }
            public static void Error(object tag, Exception e) { Console.Error.WriteLine(e); }
        #endif
    }
}