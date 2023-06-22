using DataCollectors.ClientLibrary.Options;
using Microsoft.Extensions.Logging;

namespace DataCollectors.ClientLibrary.Factories.Logging;

public class CustomLoggerFactory : ICustomLoggerFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public CustomLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public ILogger CreateLogger<T>(WebApi? webApi)
    {
        var loggerName = $"{typeof(T).FullName}.{webApi?.Name.ToString() ?? "Unknown"}";

        var logger = _loggerFactory.CreateLogger(loggerName);

        return logger;
    }
}
