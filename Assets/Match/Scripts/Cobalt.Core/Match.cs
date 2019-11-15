using System;

namespace Cobalt.Core
{
    public class Match
    {
        public MatchState State;
        public IMatchSystem[] Systems;

        public Match()
        {            
            Systems = new IMatchSystem[] {
                new InitSystem(),
                new UnitMoveSystem(),
            };

            State = new MatchState {
                inputs = new [] { new UnitInput() { move = Direction.None } },
                units = new [] { new Unit() { moveSpeed = 3f } },
            };
        }

        public T Get<T>()
        {
            foreach (var system in Systems)
                if (system is T)
                    return (T)system;
                
            throw new Exception();
        }

        public void Tick(float sec)
        {
            State.timestamp += sec;

            foreach (var system in Systems)
                system.Tick(this, sec);
        }
    }
}