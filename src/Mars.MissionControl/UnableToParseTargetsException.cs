namespace Mars.MissionControl;

[Serializable]
internal class UnableToParseTargetsException : Exception
{
	public UnableToParseTargetsException()
	{
	}

	public UnableToParseTargetsException(string? message) : base(message)
	{
	}

	public UnableToParseTargetsException(string? message, Exception? innerException) : base(message, innerException)
	{
	}

	protected UnableToParseTargetsException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}