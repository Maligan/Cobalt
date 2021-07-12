namespace Netouch
{
    public enum GestureState
    {
        Possible,
        Failed,
        
        // Discrete-only
        Accepted,
        
        // Continuous-only
        Began,
        Changed,
        Ended
    }
}