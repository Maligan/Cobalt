namespace Netouch
{
    public enum GestureState
    {
        None,
        Possible,
        Failed,
        
        // Discrete-only
        Recognized,
        
        // Continuous-only
        Began,
        Changed,
        Ended
    }
}