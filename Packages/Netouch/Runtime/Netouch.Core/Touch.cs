namespace Netouch.Core
{
    public class Touch
    {
        public int Id;
        public TouchPhase Phase;
        
        public float BeginTime;
        public float BeginX;
        public float BeginY;
        
        public float PrevTime;
        public float PrevX;
        public float PrevY;

        public float Time;
        public float X;
        public float Y;

        public float GetLength()
        {
            var dx = X - BeginX;
            var dy = Y - BeginY;
            return (float)System.Math.Sqrt(dx*dx + dy*dy);
        }
    }
}