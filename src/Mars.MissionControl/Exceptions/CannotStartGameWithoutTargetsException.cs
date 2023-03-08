namespace Mars.MissionControl.Exceptions
{
    [Serializable]
    internal class CannotStartGameWithoutTargetsException : Exception
    {
        public CannotStartGameWithoutTargetsException()
        {
        }

        public CannotStartGameWithoutTargetsException(string? message) : base(message)
        {
        }

        public CannotStartGameWithoutTargetsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CannotStartGameWithoutTargetsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}