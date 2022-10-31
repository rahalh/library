namespace Media.API.Core.Exceptions
{
    using System;

    public class NotFoundException : Exception
    {
        /// <summary>
        /// Creates a new instance of a NotFoundException object, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public NotFoundException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of a NotFoundException object, initializes it with the specified arguments.
        /// /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
