using System.Collections.Generic;

namespace Cobalt.Core
{
    public class MatchTimeline
    {
        public MatchTimeline()
        {
            States = new List<MatchState>();
            Capacity = 16;
        }

        public List<MatchState> States { get; private set; }
        public float Time { get; set; }
        public float Length => States.Count > 0 ? (States[States.Count-1].timestamp - Time) : 0;
        public int Capacity { get; set; }
        public int Count => States.Count;
        public MatchState this[int index] => States[index];

        public void Add(MatchState state)
        {
            States.Add(state);
            
            if (States.Count > Capacity)
            {
                States.RemoveAt(0);
    			Log.Warning(this, "Capacity overhead");
            }

            States.Sort(Sorter);
        }

        public void Purge()
        {
            while (Count > 2 && Time > States[1].timestamp)
                States.RemoveAt(0);
        }

        private int Sorter(MatchState s1, MatchState s2)
        {
            var delta = s1.timestamp - s2.timestamp;
            if (delta > 0) return +1;
            if (delta < 0) return -1;
            return 0; 
        }
    }
}