using System.Runtime.Serialization;

namespace Mars.MissionControl
{
    [Serializable]
    public class TooManyPlayersException : Exception
    {
        public TooManyPlayersException()
        {
        }

        public TooManyPlayersException(string? message) : base(message)
        {
        }

        public TooManyPlayersException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TooManyPlayersException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}