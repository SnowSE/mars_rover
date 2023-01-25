using System.Runtime.Serialization;

namespace Mars.MissionControl
{
    [Serializable]
    internal class UnableToUpdateBoardException : Exception
    {
        public UnableToUpdateBoardException()
        {
        }

        public UnableToUpdateBoardException(string? message) : base(message)
        {
        }

        public UnableToUpdateBoardException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UnableToUpdateBoardException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}