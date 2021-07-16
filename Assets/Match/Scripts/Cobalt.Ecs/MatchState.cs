using ProtoBuf;

namespace Cobalt.Ecs
{
    [ProtoContract]
    public class MatchState
    {
        [ProtoMember(1)] public int time;
        [ProtoMember(2)] public Unit[] units;
        public UnitInput[] inputs;
        public bool[,] walls;
    }

    [ProtoContract]
    public class UnitInput
    {
        [ProtoMember(1)]
        public Direction move;

        [ProtoMember(2)] // TODO: Без него пакет с Direction.None не передаётся т.к. там 0 
        public bool flag = true;
    }

    [ProtoContract]
    public class Unit
    {
        public enum State
        {
            Idle,
            Move,
            Dig,
            Die,
        }

        [ProtoMember(1)]
        public Vec2f pos;
        public State state;

        public Direction moveDirection;
        public float moveSpeed = 3;
        public Vec2f moveFrom;
        public Vec2f moveTo;
        public float moveProgress;

        public float digSpeed = 2;
        public float digProgress;
    }    
}