using System;
using System.Collections.Generic;

namespace Cobalt.Ecs
{
    // TODO: CAPACITY

    public class MatchTimeline
    {
        private SortedList<float, MatchState> states = new SortedList<float, MatchState>();
        private float time;
        private float tfactor;

        /// Проигрывание было начато (т.е. AdvanceTime() перемещает внутренний курсор времени)
        public bool IsStarted => time != 0;
        /// Запас времени от текущего момента (time) до самого последнего полученного кадра 
        public float Latency => states.Count > 0 ? (states.Keys[states.Count-1] - time) : 0;
        /// Максимальный запас времени (при превышении данного порога необходимо ускорить проигрывание)
        public float LatencyNormal => 0.1f;

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
                if (states.Count < 2) return;
                // Набор буффера перед началом проигрывания
                if (Latency < LatencyNormal) return;
            }

            // Движение времени
            if (IsStarted)  time = time + delta;
            else            time = states.Keys[0];

            // Если данных пришло больше чем нужно - догоняем
            if (Latency > LatencyNormal) time += (Latency - LatencyNormal);

            Purge();
        }

        public void Add(MatchState state)
        {
            states.Add(state.timestamp, state);
            Purge();
        }

        private void Purge()
        {
            if (states.Count < 2) return;

            while (states.Count > 2 && time > states.Keys[1])
                states.RemoveAt(0);

            if (IsStarted)
            {
                var total = states.Keys[1] - states.Keys[0];
                var delta = time           - states.Keys[0];
                tfactor = delta / total;

                if (tfactor < 0 || tfactor > 1)
                    Log.Warning(this, $"Interpolation t-factor is out of range (0; 1): {tfactor}");
            }

            // Output
            // Log.Info(this, $"{states.Count-1}k/{(int)(Latency*1000)}ms");
        }

        //
        // Interpolators
        //

        public int NumUnits => states.Values[0].units.Length;

        public Vec2f GetPosition(int unitIndex)
        {
            if (!IsStarted)
                throw new InvalidOperationException();
            
            var prevPos = states.Values[0].units[unitIndex].pos;
            var nextPos = states.Values[1].units[unitIndex].pos;
            return Vec2f.Lerp(prevPos, nextPos, tfactor);
        }

        public Vec2f[] GetPositions(int unitIndex)
        {
            if (!IsStarted)
                throw new InvalidOperationException();
            
            var result = new List<Vec2f>();

            foreach (var state in states.Values)
                result.Add(state.units[unitIndex].pos);

            return result.ToArray();
        }
    }
}