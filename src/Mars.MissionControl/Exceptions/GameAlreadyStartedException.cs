namespace Mars.MissionControl.Exceptions;

public class GameAlreadyStartedException : Exception
{
    public GameAlreadyStartedException()
    {
    }

    public GameAlreadyStartedException(string? message) : base(message)
    {
    }

    public GameAlreadyStartedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected GameAlreadyStartedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}