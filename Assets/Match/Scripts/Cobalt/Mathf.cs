using ProtoBuf;

namespace Cobalt
{
    public static class Mathf
    {
        public static int RoundToInt(float value) => (int)System.Math.Round(value);
        public static int FloorToInt(float value) => (int)System.Math.Floor(value);
        public static float Lerp(float a, float b, float t) => a + (b-a) * t;
        public static float Abs(float value) => System.Math.Abs(value);
    }

    [ProtoContract]
    public struct Vec2f
    {
        [ProtoMember(1)] public float x;
        [ProtoMember(2)] public float y;

        public Vec2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vec2f Round(Vec2f v)
        {
            return new Vec2f
            {
                x = Mathf.RoundToInt(v.x),
                y = Mathf.RoundToInt(v.y)
            };
        }

        public static Vec2f Lerp(Vec2f a, Vec2f b, float t)
        {
            return new Vec2f
            {
                x = Mathf.Lerp(a.x, b.x, t),
                y = Mathf.Lerp(a.y, b.y, t),
            };
        }

        #if UNITY_ENGINE
        public static implicit operator UnityEngine.Vector2(Vec2f v) => new UnityEngine.Vector2(v.x, v.y);
        public static implicit operator UnityEngine.Vector3(Vec2f v) => new UnityEngine.Vector3(v.x, v.y, 0);
        #endif
    }
}