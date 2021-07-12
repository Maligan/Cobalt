using System;
using System.Collections;

namespace Netouch.Core
{
    public interface IHitTester
    {
        // Object can be handled by this hit tester
		bool CanTest(object target);

        /// Perform hit test
        object HitTest(float x, float y);

        /// Get all target's ancestors (including target) which are valid possible hit targets (CanTest() call returns true for each one)
        IEnumerable GetHierarhy(object target);
    }
}
