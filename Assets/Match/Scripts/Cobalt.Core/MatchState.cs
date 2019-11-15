using ProtoBuf;

namespace Cobalt.Core
{
    [ProtoContract]
    public class MatchState
    {
        [ProtoMember(1)] public float timestamp;
        [ProtoMember(2)] public Unit[] units;
        public UnitInput[] inputs;
        // public BitArray walls;
    }

    [ProtoContract]
    public class UnitInput
    {
        [ProtoMember(1)]
        public Direction move;
    }

    [ProtoContract]
    public class Unit
    {
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
        public float moveSpeed;
        public Vec2f moveFrom;
        public Vec2f moveTo;
        public float moveProgress;
    }    
}