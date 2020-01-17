using System;
using System.Collections;

namespace Netouch.Input
{
    public interface IHitTester
    {
        // Base class for all possible hit targets
        Type Type { get; }

        /// Perform hit test
        object HitTest(float x, float y);

        /// Get all target's ancestors (including target) which are valid possible hit targets
        IEnumerable GetHierarhy(object target);
    }
}