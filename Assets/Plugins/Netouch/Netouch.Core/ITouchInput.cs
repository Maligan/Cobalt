using System;

namespace Netouch.Core
{
    public interface ITouchInput
    {
        event Action<Touch> Touch;
    }
}
