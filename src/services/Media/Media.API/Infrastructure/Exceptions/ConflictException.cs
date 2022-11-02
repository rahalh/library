namespace Media.API.Infrastructure.Exceptions
{
    using System;

    public class ConflictException : Exception
    {
        /// <summary>
        /// Creates a new instance of a ConflictException object, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ConflictException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of a ConflictException object, initializes it with the specified arguments.
        /// /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public ConflictException(string message, Exception innerException) : base(message, innerException) { }
    }

}
