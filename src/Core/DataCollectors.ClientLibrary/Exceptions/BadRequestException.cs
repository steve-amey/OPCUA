using System.Diagnostics.CodeAnalysis;

namespace DataCollectors.ClientLibrary.Exceptions;

[ExcludeFromCodeCoverage]
public class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message)
    {
    }
}
