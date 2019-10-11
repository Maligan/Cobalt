using System;

namespace Cobalt
{
    public static class Log
    {
        #if UNITY_EDITOR || UNITY_ANDROID
            public static void Info(string format, params object[] args) { UnityEngine.Debug.LogFormat(format, args); }
            public static void Warning(string format, params object[] args) { UnityEngine.Debug.LogWarningFormat(format, args); }
            public static void Error(Exception e) { UnityEngine.Debug.LogError(e); }
        #else
            public static void Info(string format, params object[] args) { Console.WriteLine(format, args); }
            public static void Warning(string format, params object[] args) { Console.WriteLine(format, args); }
            public static void Error(Exception e) { Console.WriteLine(e); }
        #endif
    }
}