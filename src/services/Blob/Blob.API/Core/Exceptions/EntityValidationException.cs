namespace Blob.API.Core.Exceptions
{
    using System;
    using System.Collections.Generic;

    public class EntityValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public EntityValidationException(string message)
            : base(message) {}

        public EntityValidationException(IDictionary<string, string[]> errors)
            : base(errors.ToString()) => this.Errors = errors;
    }
}
