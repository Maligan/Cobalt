using System;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Cobalt.Core
{
    public static class Utils
    {
        public static void Log(string format, params object[] args)
        {
            #if UNITY_EDITOR
                Debug.LogFormat(format, args);
            #else
                Console.WriteLine(format, args);
            #endif
        }
    }
} 