using DataCollectors.ClientLibrary.Options;
using Microsoft.Extensions.Logging;

namespace DataCollectors.ClientLibrary.Factories.Logging;

public interface ICustomLoggerFactory
{
    ILogger CreateLogger<T>(WebApi? webApi);
}
