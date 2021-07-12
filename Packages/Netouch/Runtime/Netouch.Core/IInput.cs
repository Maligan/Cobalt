using System;

namespace Netouch.Core
{
    public interface IInput
    {
        // User makes a touch
        event Action<Touch> Touch;

        // System does frame processing
		event Action<float> Frame;
    }
}
