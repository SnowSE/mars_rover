using System.Runtime.Serialization;

namespace Mars.MissionControl
{
    [Serializable]
    internal class UnableToGenerateBoardException : Exception
    {
        public UnableToGenerateBoardException()
        {
        }

        public UnableToGenerateBoardException(string? message) : base(message)
        {
        }

        public UnableToGenerateBoardException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UnableToGenerateBoardException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}