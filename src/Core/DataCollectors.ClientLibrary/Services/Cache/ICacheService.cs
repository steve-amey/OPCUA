using DataCollectors.ClientLibrary.Services.Configuration.Responses;

namespace DataCollectors.ClientLibrary.Services.Cache;

public interface ICacheService : ITransientService
{
    Task<ConfigurationDto?> GetConfiguration(CancellationToken cancellationToken);

    Task<ConfigurationDto?> UpdateConfiguration(string value, CancellationToken cancellationToken);
}
