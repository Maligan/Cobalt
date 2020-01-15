using System.Numerics;

namespace GestureKit.Core
{
    public interface IHitTester
    {
        object HitTest(Vector2 point, object possibleTarget = null);
    }
}