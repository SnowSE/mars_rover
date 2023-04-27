using System.Runtime.Serialization;

namespace Mars.Web;
[Serializable]
internal class UnableToDeserializeLowResolutionCellException : Exception
{
	public UnableToDeserializeLowResolutionCellException()
	{
	}

	public UnableToDeserializeLowResolutionCellException(string? message) : base(message)
	{
	}

	public UnableToDeserializeLowResolutionCellException(string? message, Exception? innerException) : base(message, innerException)
	{
	}

	protected UnableToDeserializeLowResolutionCellException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}