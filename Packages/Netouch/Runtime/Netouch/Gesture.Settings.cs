using System;
using System.Collections.Generic;
using Netouch.Core;

namespace Netouch
{
    public partial class Gesture
    {
        /// Screen DPI
        public static int Dpi { get; set; } = 120;
        
        /// Threshold for touch movement (based on 20 pixels on a 252ppi device.)
        public static int Slop => (int)(20 * Dpi/252f + 0.5f);

        private static IInput input;
        private static SortedList<int, IHitTester> hitTesters = new SortedList<int, IHitTester>();

        public static void Add(IInput value)
        {
            if (input != null)
                throw new InvalidOperationException("IInput already assigned");
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            input = value;
            input.Frame += OnProcessFrame;
            input.Touch += OnProcessTouch;
        }

        public static void Add(IHitTester hitTester, byte priority = 0)
        {
            if (hitTester == null)
                throw new ArgumentNullException(nameof(hitTester));
            
            if (hitTesters.Values.Contains(hitTester))
                return;

            if (hitTesters.Count == 0xFF)
                throw new InvalidOperationException("Hit testers limit exhausted (max 255)");

            // Recently added has lower priority than older
            var key = (priority << 8) | (0xFF - hitTesters.Count);

            hitTesters.Add(key, hitTester);
        }
    }
}