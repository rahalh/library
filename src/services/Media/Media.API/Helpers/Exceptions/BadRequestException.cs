namespace Media.API.Core.Helpers.Exceptions
{
    using System;

    public class BadRequestException : Exception
    {
        /// <summary>
        /// Creates a new instance of a BadRequestException object, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public BadRequestException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of a BadRequestException object, initializes it with the specified arguments.
        /// /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public BadRequestException(string message, Exception innerException) : base(message, innerException) { }
    }
}
