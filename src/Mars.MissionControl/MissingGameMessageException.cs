namespace Mars.MissionControl;

[Serializable]
internal class MissingGameMessageException : Exception
{
	public MissingGameMessageException()
	{
	}

	public MissingGameMessageException(string? message) : base(message)
	{
	}

	public MissingGameMessageException(string? message, Exception? innerException) : base(message, innerException)
	{
	}

	protected MissingGameMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}