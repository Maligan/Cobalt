using System;
using ProtoBuf;

namespace Cobalt.Core
{
    public static class Mathf
    {
        public static int RoundToInt(float value) => (int)System.Math.Round(value);
        public static int FloorToInt(float value) => (int)System.Math.Floor(value);
        public static float Lerp(float a, float b, float t) => a + (b-a) * t;
    }

    [ProtoContract]
    public struct Vec2f
    {
        [ProtoMember(1)]
        public float x;
        [ProtoMember(2)]
        public float y;

        public Vec2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vec2f Lerp(Vec2f a, Vec2f b, float t)
        {
            return new Vec2f
            {
                x = Mathf.Lerp(a.x, b.x, t),
                y = Mathf.Lerp(a.y, b.y, t),
            };
        }
    }
}