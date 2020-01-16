using System;
using System.Collections;

namespace GestureKit.Input
{
    public interface IHitTester
    {
        Type Type { get; }
        IEnumerable HitTest(float x, float y);
    }
}