using System.Runtime.Serialization;

namespace Mars.Web;
[Serializable]
internal class EmptyMapContentException : Exception
{
	public EmptyMapContentException()
	{
	}

	public EmptyMapContentException(string? message) : base(message)
	{
	}

	public EmptyMapContentException(string? message, Exception? innerException) : base(message, innerException)
	{
	}

	protected EmptyMapContentException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}