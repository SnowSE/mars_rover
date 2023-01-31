namespace Mars.MissionControl
{
    [Serializable]
    internal class UnableToUpdatePlayerException : Exception
    {
        public UnableToUpdatePlayerException()
        {
        }

        public UnableToUpdatePlayerException(string? message) : base(message)
        {
        }

        public UnableToUpdatePlayerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UnableToUpdatePlayerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}