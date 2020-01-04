using System;
using System.Collections.Generic;

namespace Cobalt.Ecs
{
    public class Match
    {
        public MatchState State;
        public List<IMatchSystem> Systems;

        public Match()
        {            
            Systems = new List<IMatchSystem> {
                new InitSystem(),
                new UnitAISystem(),
                new UnitMoveSystem(),
            };

            State = new MatchState {
                inputs = new [] {
                    new UnitInput(),
                    new UnitInput() { flag = false }
                },
                units = new [] {
                    new Unit(),
                    new Unit()
                },
            };
        }

        public T Get<T>()
        {
            foreach (var system in Systems)
                if (system is T)
                    return (T)system;
                
            throw new Exception();
        }

        public void Add(IMatchSystem system)
        {
            Systems.Add(system);
        }

        public void Tick(float sec)
        {
            State.timestamp += sec;

            foreach (var system in Systems)
                system.Tick(this, sec);
        }
    }
}