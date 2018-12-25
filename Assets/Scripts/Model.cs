using System;
using System.Collections.Generic;

namespace Server
{
    public class Stage
    {
        public List<StageObject> Objects = new List<StageObject>();

        public void Update(int ms)
        {
            foreach (var obj in Objects)
                obj.Update(ms);
        }
    }

    public enum DataType { Wall, Unit, Bomb, Coin }
    public class Data
    {
        public int ID;
        public DataType Type;
        
        public int X;
        public int Y;

        public int DX;
        public int DY;

        // public short uid;
        // public short pos;
        // public byte info;

        // public DataType Type { get { return (DataType)(info & 0x3); } set { info |= (byte)value; } }
        // public byte X { get { return (byte)(pos & 0xFF); } set { pos |= (short)value; } }
        // public byte Y { get { return (byte)(pos >> 8); } set { pos |= (short)(value << 8); } }
    }

    public abstract class StageObject
    {
        public Data Data;
        public readonly Stage Stage;

        public StageObject(Stage stage)
        {
            Stage = stage;
            Data = new Data();
        }

        public abstract void Update(int ms);
    }

    public class StageUnit : StageObject
    {
        public StageUnit(Stage stage) : base(stage)
        {
            Data.Type = DataType.Unit;
        }

        public override void Update(int ms)
        {
            Data.X += Data.DX;
            Data.Y += Data.DY;
        }

        // public bool IsMoving      { get { return (Data.info & 0x7) != 0x0; } }
        // public bool IsMovingUp    { get { return (state & 0x7) == 0x1; } }
        // public bool IsMovingRight { get { return (state & 0x7) == 0x2; } }
        // public bool IsMovingDown  { get { return (state & 0x7) == 0x3; } }
        // public bool IsMovingLeft  { get { return (state & 0x7) == 0x4; } }
    }
}