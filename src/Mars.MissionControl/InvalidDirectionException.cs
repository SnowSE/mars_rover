namespace Mars.MissionControl;

[Serializable]
internal class InvalidDirectionException : Exception
{
	public InvalidDirectionException()
	{
	}

	public InvalidDirectionException(string? message) : base(message)
	{
	}

	public InvalidDirectionException(string? message, Exception? innerException) : base(message, innerException)
	{
	}

	protected InvalidDirectionException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}