using System.Diagnostics.CodeAnalysis;

namespace DataCollectors.ClientLibrary.Exceptions;

[ExcludeFromCodeCoverage]
public class ValidationException : Exception
{
    public ValidationException()
    {
    }

    public ValidationException(string message)
        : base(message)
    {
        Errors[string.Empty] = message;
    }

    public ValidationException(IDictionary<string, string> errors)
        : this()
    {
        Errors = errors;
    }

    public IDictionary<string, string> Errors { get; } = new Dictionary<string, string>();
}
