using System;

namespace GestureKit.Input
{
    public interface ITouchInput
    {
        event Action<Touch> Touch;
    }
}