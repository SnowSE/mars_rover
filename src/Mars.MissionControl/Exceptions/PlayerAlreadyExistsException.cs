namespace Mars.MissionControl.Exceptions
{
    [Serializable]
    internal class PlayerAlreadyExistsException : Exception
    {
        public PlayerAlreadyExistsException()
        {
        }

        public PlayerAlreadyExistsException(string? message) : base(message)
        {
        }

        public PlayerAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected PlayerAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}