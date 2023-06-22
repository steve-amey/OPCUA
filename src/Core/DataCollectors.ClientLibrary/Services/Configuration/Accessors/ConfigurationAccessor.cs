using System.Text.Json;
using DataCollectors.ClientLibrary.Services.Cache;
using DataCollectors.ClientLibrary.Services.Configuration.Responses;
using DataCollectors.ClientLibrary.Services.External.Configuration;
using DataCollectors.ClientLibrary.Services.Serialization;
using Microsoft.Extensions.Logging;

namespace DataCollectors.ClientLibrary.Services.Configuration.Accessors;

public class ConfigurationAccessor : IConfigurationAccessor
{
    private readonly ICacheService _cacheService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger _logger;

    public ConfigurationAccessor(ICacheService cacheService, IConfigurationService configurationService, ILogger<ConfigurationAccessor> logger)
    {
        _cacheService = cacheService;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<ConfigurationDto?> GetConfiguration(CancellationToken cancellationToken = default)
    {
        var result = await _cacheService.GetConfiguration(cancellationToken);

        if (result != null)
        {
            _logger.LogDebug("Returning configuration loaded from cache");

            return result;
        }

        _logger.LogDebug("Getting configuration from service...");

        var serviceResponse = await _configurationService
            .GetConfiguration();

        var configuration = serviceResponse.Result?.FirstOrDefault();

        if (string.IsNullOrEmpty(configuration))
        {
            _logger.LogError(JsonSerializer.Serialize(serviceResponse, CustomJsonSerializerOptions.Standard));
        }
        else
        {
            _logger.LogDebug("Setting configuration in cache");

            result = await _cacheService
                .UpdateConfiguration(configuration, cancellationToken);
        }

        return result;
    }
}
