using DataCollectors.OPCUA.Core.Application.Shared.Enums;

namespace DataCollectors.OPCUA.Core.Application.Shared.Exceptions;

/// <summary>
/// An exception that occurred and caused an exit of the application.
/// </summary>
public class ErrorExitException : Exception
{
    public ErrorExitException(ExitCode exitCode)
    {
        ExitCode = exitCode;
    }

    public ErrorExitException()
    {
        ExitCode = ExitCode.Ok;
    }

    public ErrorExitException(string message)
        : base(message)
    {
        ExitCode = ExitCode.Ok;
    }

    public ErrorExitException(string message, ExitCode exitCode)
        : base(message)
    {
        ExitCode = exitCode;
    }

    public ErrorExitException(string message, Exception innerException)
        : base(message, innerException)
    {
        ExitCode = ExitCode.Ok;
    }

    public ErrorExitException(string message, Exception innerException, ExitCode exitCode)
        : base(message, innerException)
    {
        ExitCode = exitCode;
    }

    public ExitCode ExitCode { get; }
}
