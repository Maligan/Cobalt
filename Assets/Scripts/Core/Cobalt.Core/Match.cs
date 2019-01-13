using System;
using System.Collections.Generic;
using ProtoBuf;
using Cobalt.Core;

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
                        move = Unit.Direction.Right
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
            State.timestamp += sec;

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

    [ProtoContract]
    public class UnitInput
    {
        [ProtoMember(1)]
        public Unit.Direction move;
        [ProtoMember(2)]
        public bool t = true;
    }

    [ProtoContract]
    public class Unit
    {
        public enum Direction
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
        public Vec2f pos;
        public State state;

        public Direction moveDirection;
        public float moveProgress;
        public Vec2f moveFrom;
        public Vec2f moveTo;
        public float moveSpeed;
    }

    // Components

    // public interface ITransform
    // {
    //     float x { get; set; }
    //     float y { get; set; }
    // }

    // Systems

    public interface IMatchSystem
    {
        void Tick(Match match, float sec);
    }

    public class UnitSystem : IMatchSystem
    {
        private void Fix(Unit unit)
        {
            // unit.x = Mathf.RoundToInt(unit.x);
            // unit.y = Mathf.RoundToInt(unit.y);

            // switch (unit.moveDirection)
            // {
            //     case Unit.Direction.Top:    unit.y += unit.moveProgress; break;
            //     case Unit.Direction.Bottom: unit.y -= unit.moveProgress; break;
                
            //     case Unit.Direction.Right:  unit.x += unit.moveProgress; break;
            //     case Unit.Direction.Left:   unit.x -= unit.moveProgress; break;
            // }
        }

        // public void Lerp(Unit.Direction direction, float ratio)
        // {
            
        // }

        

        private static Vec2f GetNext(Vec2f v, Unit.Direction direction)
        {
            switch (direction)
            {
                case Unit.Direction.Top:    return new Vec2f(v.x, v.y+1);
                case Unit.Direction.Bottom: return new Vec2f(v.x, v.y-1);
                case Unit.Direction.Right:  return new Vec2f(v.x+1, v.y);
                case Unit.Direction.Left:   return new Vec2f(v.x-1, v.y);

                default:                    throw  new ArgumentException();
            }
        }

        public void Tick(Match match, float sec)
        {
            for (int i = 0; i < match.State.units.Length; i++)
            {
                var unit = match.State.units[i];
                var unitInput = match.State.inputs[i];

                // Tick [Idle]
                if (unit.state == Unit.State.Idle)
                {
                    if (unitInput.move != Unit.Direction.None)
                    {
                        unit.state = Unit.State.Move;

                        unit.moveDirection = unitInput.move;
                        unit.moveProgress = 0;
                        unit.moveFrom = unit.pos;
                        unit.moveTo = GetNext(unit.pos, unit.moveDirection);
                    }
                }

                // Tick [Move]
                if (unit.state == Unit.State.Move)
                {
                    unit.moveProgress += unit.moveSpeed * sec;

                    // Изменилось направление движения в процессе перемещения
                    if (unit.moveProgress <= 0.5f && unit.moveDirection != unitInput.move)
                    {
                    }
                    // Дошли до следующей ячейки
                    else if (unit.moveProgress >= 1f)
                    {
                        unit.state = Unit.State.Idle;
                        // unit.moveProgress = 0;
                        // unit.moveDirection = Unit.Direction.None;
                    }

                    unit.pos = Vec2f.Lerp(unit.moveFrom, unit.moveTo, unit.moveProgress);
                }
            }
        }
    }
}