namespace Mars.MissionControl
{
    [Serializable]
    public class InvalidFormatException : Exception
    {
        public InvalidFormatException()
        {
        }

        public InvalidFormatException(string? message) : base(message)
        {
        }

        public InvalidFormatException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}