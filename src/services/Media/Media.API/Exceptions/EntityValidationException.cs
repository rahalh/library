using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class EntityValidationException : Exception
{
    public IDictionary<string, string[]> errors;

    public EntityValidationException(string message)
        : base(message) {}

    public EntityValidationException(IDictionary<string, string[]> errors)
        : base(errors.ToString()) =>
        this.errors = errors;
}
