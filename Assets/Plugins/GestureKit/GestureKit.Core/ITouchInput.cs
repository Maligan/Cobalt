using System;

namespace GestureKit.Core
{
    public interface ITouchInput
    {
        event Action<Touch> Touch;
    }
}