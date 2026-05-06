public struct TransitionResult
{
    public bool Success { get; }
    public CrowState OldState { get; }
    public CrowState NewState { get; }
    public string Message { get; }

    public TransitionResult(bool success, CrowState oldState, CrowState newState, string message)
    {
        Success = success;
        OldState = oldState;
        NewState = newState;
        Message = message;
    }
}