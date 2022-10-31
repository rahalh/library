namespace Blob.API.Core.Exceptions
{
    using System;
    using System.Collections.Generic;

    public class ValidationException : Exception
    {
        /// <summary>
        /// Creates a new instance of a ValidationException object, initializes it with the specified arguments.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ValidationException(string message) : base(message) { }

        /// <summary>
        /// Creates a new instance of a ValidationException object, initializes it with the specified arguments.
        /// /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <returns></returns>
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
