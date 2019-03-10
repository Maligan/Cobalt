using System;
using System.Collections.Generic;
using ProtoBuf;
using Cobalt.Core;
using System.Collections;

namespace Cobalt.Core
{
    public class Match
    {
        public MatchState State;
        public IMatchSystem[] Systems;

        public Match()
        {
            var cave = MatchBuilder.Random(21, 19);
            var caveBits = new BitArray(cave.Length);
            var caveW = cave.GetLength(0);
            var caveH = cave.GetLength(1);

            for (var x = 0; x < caveW; x++)
                for (var y = 0; y < caveH; y++)
                    caveBits[x * caveH + y] = cave[x, y];
            
            Systems = new [] {
                new UnitSystem(),
            };

            State = new MatchState {
                inputs = new [] {
                    new UnitInput() {
                        move = Unit.Direction.None
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
    			Log.Warning("[MatchTimeline] Capacity overhead");
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
        [ProtoMember(1)] public float timestamp;
        [ProtoMember(2)] public Unit[] units;
        public UnitInput[] inputs;

        // [ProtoMember(3)]
        // public BitArray walls;
    }

    /*
    public class MatchStatePacket
    {
        public int index;

        public     int[] create;
        public     int[] remove;
        public  object[] update;
    }
    */

    //
    // Entities
    //

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

    //
    // Components
    //

    // public interface ITransform
    // {
    //     float x { get; set; }
    //     float y { get; set; }
    // }

    //
    // Systems
    //

    public interface IMatchSystem
    {
        void Tick(Match match, float sec);
    }

    public class UnitSystem : IMatchSystem
    {
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
                        TryMoveTo(unit, unitInput.move, 0);
                    }
                }

                // Tick [Move]
                if (unit.state == Unit.State.Move)
                {
                    unit.moveProgress += unit.moveSpeed * sec;

                    // Дошли до следующей ячейки
                    if (unit.moveProgress >= 1f)
                    {
                        if (unitInput.move == unit.moveDirection)
                        {
                            TryMoveTo(unit, unit.moveDirection, unit.moveProgress % 1);
                        }
                        else if (unitInput.move != unit.moveDirection)
                        {
                            unit.state = Unit.State.Idle;
                            unit.moveProgress = 1;
                        }
                    }

                    unit.pos = Vec2f.Lerp(unit.moveFrom, unit.moveTo, unit.moveProgress);
                }
            }
        }

        public void TryMoveTo(Unit unit, Unit.Direction direction, float progress)
        {
            var from = Vec2f.Round(unit.pos);
            var to = GetNext(from, direction);

            var canPass = CanPassTo(unit, from, to);
            if (canPass)
            {
                unit.state = Unit.State.Move;
                unit.moveDirection = direction;
                unit.moveFrom = Vec2f.Round(unit.pos);
                unit.moveTo = GetNext(unit.moveFrom, direction);
                unit.moveProgress = progress;
            }
            else
            {
                unit.state = Unit.State.Idle;
            }
        }

        public bool CanPassTo(Unit unit, Vec2f from, Vec2f to)
        {
            return true;
        }
    }
}