namespace Mars.MissionControl.Exceptions;

[Serializable]
public class BoardTooSmallException : Exception
{
    public BoardTooSmallException()
    {
    }

    public BoardTooSmallException(string? message) : base(message)
    {
    }

    public BoardTooSmallException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected BoardTooSmallException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
