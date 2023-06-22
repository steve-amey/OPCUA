using System.Diagnostics.CodeAnalysis;

namespace DataCollectors.ClientLibrary.Exceptions;

[ExcludeFromCodeCoverage]
public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
