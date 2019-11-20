using System;
using System.Collections.Generic;

namespace Cobalt.Ecs
{
    // TODO: CAPACITY

    public class MatchTimeline
    {
        private List<MatchState> buffer;
        private float time;
        private float t;

        public MatchTimeline()
        {
            buffer = new List<MatchState>();
        }

        public bool IsEmpty => buffer.Count == 0;
        public bool IsStarted => time != 0;
        public float Latency => buffer.Count > 0 ? (buffer[buffer.Count-1].timestamp - time) : 0;

        public void AdvanceTime(float delta)
        {
            if (IsStarted)
            {
                // Вышли за пределы временного буффера (нужно ждать, либо экстраполировать)
                if (Latency < delta) return;
            }
            else
            {
                // Интерполяцию и проигрывание начинается только когда накопим хотя бы 2 кадра
                if (buffer.Count < 2) return;
                // Набор буффера перед началом проигрывания
                if (Latency < 0.1f) return;
            }

            // Движение времени
            if (IsStarted)  time = time + delta;
            else            time = buffer[0].timestamp;
            Purge();
        }

        public void Add(MatchState state)
        {
            buffer.Add(state);
            buffer.Sort(Sorter);
            Purge();

            // $"{buffer.Count-1}k/{(int)(Lag*1000)}ms"
        }

        private int Sorter(MatchState s1, MatchState s2)
        {
            var delta = s1.timestamp - s2.timestamp;
            if (delta > 0) return +1;
            if (delta < 0) return -1;
            return 0;
        }

        private void Purge()
        {
            const int historySize = 1;
            if (buffer.Count < historySize + 1) return;

            while (buffer.Count > historySize+1 && time > buffer[historySize].timestamp)
                buffer.RemoveAt(0);

            if (IsStarted)
            {
                var total = buffer[1].timestamp - buffer[0].timestamp;
                var delta = time                - buffer[0].timestamp;
                t = delta / total;

                if (t < 0 || t > 1)
                    Log.Warning(this, $"Interpolation t-factor is out of range (0; 1): {t}");
            }
        }

        //
        // Interpolators
        //

        public Vec2f GetPosition(int unitIndex)
        {
            if (IsEmpty)
                throw new InvalidOperationException("Timeline is empty");
            
            if (buffer.Count == 1)
                return buffer[0].units[unitIndex].pos;

            var prevPos = buffer[0].units[unitIndex].pos;
            var nextPos = buffer[1].units[unitIndex].pos;
            return Vec2f.Lerp(prevPos, nextPos, t);
        }
    }
}