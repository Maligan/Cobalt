namespace GestureKit
{
    public enum GestureState
    {
        Idle,
        Possible,
        Failed,
        
        // Discrete
        Recognized,
        
        // Continuous
        Began,
        Changed,
        Ended
    }
}