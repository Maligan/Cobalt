using System;

namespace Netouch.Input
{
    public interface ITouchInput
    {
        event Action<Touch> Touch;
    }
}