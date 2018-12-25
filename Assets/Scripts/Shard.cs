using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Cobalt.Shard
{
    public class Shard
    {
        public ShardState State;
        public ShardSystem[] Systems;

        public Shard()
        {
            Systems = new [] {
                new UnitSystem(),
            };

            State = new ShardState {
                inputs = new [] {
                    new Input() {
                        move = Unit.Rotation.Right
                    },
                },

                units = new [] {
                    new Unit() {
                        moveSpeed = 1f
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
            
            State.timestamp += sec;
        }
    }

    [ProtoContract]
    public class ShardState
    {
        [ProtoMember(1)]
        public float timestamp;
        [ProtoMember(2)]
        public Unit[] units;
        public Input[] inputs;
    }

    // Entities

    public class Input
    {
        public Unit.Rotation move;
    }

    [ProtoContract]
    public class Unit : IPos
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

    public interface IPos
    {
        float x { get; set; }
        float y { get; set; }
    }

    // Systems

    public interface ShardSystem
    {
        void Tick(Shard shard, float sec);
    }

    public class UnitSystem : ShardSystem
    {
        public void Tick(Shard shard, float sec)
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
                            unit.state = Unit.State.Idle;
                            unit.moveProgress = 0;
                        }
                        else
                        {
                            unit.moveProgress %= 1;
                        }
                    }

                    // Координаты для клиента
                    unit.x = unit.cellX + dx*unit.moveProgress;
                    unit.y = unit.cellY + dy*unit.moveProgress;
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


