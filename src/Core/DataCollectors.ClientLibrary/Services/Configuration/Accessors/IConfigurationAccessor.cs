using DataCollectors.ClientLibrary.Services.Configuration.Responses;

namespace DataCollectors.ClientLibrary.Services.Configuration.Accessors;

public interface IConfigurationAccessor : ITransientService
{
    Task<ConfigurationDto?> GetConfiguration(CancellationToken cancellationToken = default);
}
