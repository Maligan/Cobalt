using System;

namespace Cobalt.Core
{
    // 3-bit direction enum
    public enum Direction
    {
        None = 0,   // 000
        Top = 4,    // 100
        Right = 5,  // 101
        Bottom = 6, // 110
        Left = 7,   // 111
    }

    public static class DirectionExtension
    {
        public static Vec2f GetNext(this Vec2f v, Direction direction)
        {
            switch (direction)
            {
                case Direction.None:   return v;
                case Direction.Top:    return new Vec2f(v.x, v.y+1);
                case Direction.Bottom: return new Vec2f(v.x, v.y-1);
                case Direction.Right:  return new Vec2f(v.x+1, v.y);
                case Direction.Left:   return new Vec2f(v.x-1, v.y);
            }

            throw new ArgumentException();
        }
    }
}