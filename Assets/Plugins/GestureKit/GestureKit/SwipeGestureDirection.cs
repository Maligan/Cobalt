using System;

namespace GestureKit
{
    [Flags]
    public enum SwipeGestureDirection : byte
    {
        None = 0,

        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8,

        Horizontal = Right | Left,
        Vertical = Up | Down,

        Orthogonal = Horizontal | Vertical
    }
}