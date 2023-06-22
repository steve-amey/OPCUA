using DataCollectors.ClientLibrary.Services;

namespace DataCollectors.ClientLibrary.Builders.Alarm;

public interface ISourceBuilder : ITransientService
{
    Task<string> BuildSource(string? source);
}
