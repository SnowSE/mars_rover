using System.Runtime.Serialization;

namespace Mars.Web;
[Serializable]
internal class UnableToDeserializeTerrainException : Exception
{
	public UnableToDeserializeTerrainException()
	{
	}

	public UnableToDeserializeTerrainException(string? message) : base(message)
	{
	}

	public UnableToDeserializeTerrainException(string? message, Exception? innerException) : base(message, innerException)
	{
	}

	protected UnableToDeserializeTerrainException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}