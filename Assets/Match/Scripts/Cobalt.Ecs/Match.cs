using System;
using System.Collections.Generic;

namespace Cobalt.Ecs
{
    public class Match
    {
        private List<IMatchSystem> _systems;

        public MatchState State { get; }

        public Match()
        {            
            _systems = new List<IMatchSystem> {
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
            foreach (var system in _systems)
                if (system is T)
                    return (T)system;
                
            throw new Exception();
        }

        public void Add(IMatchSystem system)
        {
            _systems.Add(system);
        }

        public void Update(int dt)
        {
            State.time += dt;

            foreach (var system in _systems)
                system.Update(this, dt);
        }
    }
}