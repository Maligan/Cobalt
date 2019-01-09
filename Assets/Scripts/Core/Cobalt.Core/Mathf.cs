using System;
using ProtoBuf;

namespace Cobalt.Core
{
    public static class Mathf
    {
        public static int RoundToInt(float value) => (int)System.Math.Round(value);
        public static int FloorToInt(float value) => (int)System.Math.Floor(value);
    }

    [ProtoContract]
    public struct Vec2f
    {
        [ProtoMember(1)]
        public float x;
        [ProtoMember(2)]
        public float y;
    }
}