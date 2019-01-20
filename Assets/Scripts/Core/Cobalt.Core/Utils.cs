using System;

namespace Cobalt.Core
{
    public static class Utils
    {
        #if UNITY_EDITOR || UNITY_ANDROID
            public static void Log(string format, params object[] args) { UnityEngine.Debug.LogFormat(format, args); }
            public static void LogWarning(string format, params object[] args) { UnityEngine.Debug.LogWarningFormat(format, args); }
            public static void LogError(Exception e) { UnityEngine.Debug.LogError(e); }
        #else
            public static void Log(string format, params object[] args) { Console.WriteLine(format, args); }
            public static void LogWarning(string format, params object[] args) { Console.WriteLine(format, args); }
            public static void LogError(Exception e) { Console.WriteLine(e); }
        #endif
    }
}