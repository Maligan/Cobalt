using System.Collections;

namespace GestureKit.Input
{
    public interface IHitTester
    {
        IEnumerable HitTest(float x, float y);
    }
}