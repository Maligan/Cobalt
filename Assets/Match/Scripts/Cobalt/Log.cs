using System;

namespace Cobalt
{
    public static class Log
    {
        #if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            public static void Info(object tag, string message) => UnityEngine.Debug.Log($"[{GetTag(tag)}] {message}");
            public static void Warning(object tag, string message) { UnityEngine.Debug.LogWarning($"[{GetTag(tag)}] {message}"); }
            public static void Error(object tag, Exception e) { UnityEngine.Debug.LogError(e); }
        #else
            public static void Info(object tag, string message) { Console.WriteLine($"[{GetTag(tag)}] {message}"); }
            public static void Warning(object tag, string message) { Console.WriteLine($"[{GetTag(tag)}] {message}"); }
            public static void Error(object tag, Exception e) { Console.Error.WriteLine(e); }
        #endif

        private static string GetTag(object tag)
        {
            if (tag == null)
                return string.Empty;

            if (tag is string)
                return (string)tag;

            return tag.GetType().Name;
        }
    }
}