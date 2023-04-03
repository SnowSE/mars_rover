namespace Mars.MissionControl.Exceptions
{
    [Serializable]
    internal class InvalidIngenuityIdException : Exception
    {
        public InvalidIngenuityIdException()
        {
        }

        public InvalidIngenuityIdException(string? message) : base(message)
        {
        }

        public InvalidIngenuityIdException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidIngenuityIdException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}