using System;

#if UNITY_EDITOR || UNITY_ANDROID
using UnityEngine;
#endif

namespace Cobalt.Core
{
    public static class Utils
    {
        public static void Log(string format, params object[] args)
        {
            #if UNITY_EDITOR || UNITY_ANDROID
                Debug.LogFormat(format, args);
            #else
                Console.WriteLine(format, args);
            #endif
        }

        public static void LogError(Exception e)
        {
            #if UNITY_EDITOR || UNITY_ANDROID
                Debug.LogError(e);
            #else
                Console.Error.WriteLine(e);
            #endif
        }
    }
} 