using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Cobalt.Core
{
    public class Match
    {
        public MatchState State;
        public IMatchSystem[] Systems;

        public Match()
        {
            Systems = new [] {
                new UnitSystem(),
            };

            State = new MatchState {
                inputs = new [] {
                    new UnitInput() {
                        move = Unit.Rotation.Right
                    },
                },

                units = new [] {
                    new Unit() {
                        moveSpeed = 3f
                    },
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

        public void Tick(float sec)
        {
            foreach (var system in Systems)
                system.Tick(this, sec);
        }
    }

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
    			Utils.Log("[MatchTimeline] Capacity overhead");
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

    [ProtoContract]
    public class MatchState
    {
        [ProtoMember(1)]
        public float timestamp;
        [ProtoMember(2)]
        public Unit[] units;
        public UnitInput[] inputs;
    }

    // Entities

    public class UnitInput
    {
        public Unit.Rotation move;
    }

    [ProtoContract]
    public class Unit : ITransform
    {
        public enum Rotation
        {
            None = 0,   // 000
            Top = 4,    // 100
            Right = 5,  // 101
            Bottom = 6, // 110
            Left = 7,   // 111
        }

        public enum State
        {
            Idle,
            Move,
            Die,
        }

        [ProtoMember(1)]
        public float x { get; set; }
        [ProtoMember(2)]
        public float y { get; set; }

        public int cellX { get; set; }
        public int cellY { get; set; }

        public Rotation rotation { get; set; }
        public State state { get; set; }

        public float moveProgress { get; set; }
        public float moveSpeed { get; set; }
    }

    // Components

    public interface ITransform
    {
        float x { get; set; }
        float y { get; set; }
    }

    // Systems

    public interface IMatchSystem
    {
        void Tick(Match shard, float sec);
    }

    public class UnitSystem : IMatchSystem
    {
        public void Tick(Match shard, float sec)
        {
            for (int i = 0; i < shard.State.units.Length; i++)
            {
                var unit = shard.State.units[i];
                var unitInput = shard.State.inputs[i];

                if (unit.state == Unit.State.Move)
                {
                    var dx = 0;
                    var dy = 0;
                    
                    switch (unit.rotation)
                    {
                        case Unit.Rotation.Top:    dy = +1; break;
                        case Unit.Rotation.Right:  dx = +1; break;
                        case Unit.Rotation.Bottom: dy = -1; break;
                        case Unit.Rotation.Left:   dx = -1; break;
                    }

                    // Движение
                    unit.moveProgress += unit.moveSpeed * sec;
                    
                    // Переход
                    if (unit.moveProgress >= 1)
                    {
                        unit.cellX += dx;
                        unit.cellY += dy;

                        if (unitInput.move == Unit.Rotation.None)
                        {
                            unitInput.move = Unit.Rotation.None;
                            unit.state = Unit.State.Idle;
                            unit.moveProgress = 0;
                        }
                        else if (unitInput.move == unit.rotation)
                        {
                            unit.moveProgress %= 1;
                        }
                        else
                        {
                            unit.rotation = unitInput.move;
                            unit.moveProgress = 0;
                        }
                    }

                    // Координаты для клиента
                    unit.x = unit.cellX + dx*unit.moveProgress;
                    unit.y = unit.cellY + dy*unit.moveProgress;

                    // unit.x = Mathf.Clamp(unit.x, -5, +5);
                    // unit.y = Mathf.Clamp(unit.y, -4, +4);
                }
                else if (unit.state == Unit.State.Idle)
                {
                    if (unitInput.move != Unit.Rotation.None)
                    {
                        unit.state = Unit.State.Move;
                        unit.rotation = unitInput.move;
                    }
                }
            }
        }
    }
}


