namespace Mars.MissionControl.Exceptions;

[Serializable]
public class UnrecognizedTokenException : Exception
{
    public UnrecognizedTokenException()
    {
    }

    public UnrecognizedTokenException(string? message) : base(message)
    {
    }

    public UnrecognizedTokenException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UnrecognizedTokenException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}