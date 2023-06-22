using DataCollectors.ClientLibrary.Extensions;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services.Configuration.Accessors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataCollectors.ClientLibrary.Builders.Alarm;

public class SourceBuilder : ISourceBuilder
{
    private readonly AppSettings _appSettings;
    private readonly IConfigurationAccessor _configurationAccessor;
    private readonly ILogger _logger;

    public SourceBuilder(IOptions<AppSettings> appSettings, IConfigurationAccessor configurationAccessor, ILogger<SourceBuilder> logger)
    {
        _appSettings = appSettings.Value;
        _configurationAccessor = configurationAccessor;
        _logger = logger;
    }

    public async Task<string> BuildSource(string? source)
    {
        var defaultValue = _appSettings.FallbackSourceId;

        var configuration = await _configurationAccessor.GetConfiguration(CancellationToken.None);

        if (configuration != null)
        {
            var flattenedList = configuration.Items.Flatten(x => x.Items).ToList();
            flattenedList.Insert(0, configuration);

            flattenedList = flattenedList.OrderBy(x => x.FullNamePath).ToList();

            _logger.LogInformation("Finding source in list: {source}", source);

            var sourceEntry = flattenedList.FirstOrDefault(x => x.FullNamePath.EndsWith($".{source}"));

            if (sourceEntry != null)
            {
                _logger.LogInformation("Found source entry: {source}", sourceEntry.FullNamePath);

                return sourceEntry.FullIdPath;
            }

            _logger.LogInformation("Could not find source entry: {source}, looking for fallback", source);

            var defaultFallback = flattenedList.FirstOrDefault(x => x.Id.ToString() == _appSettings.FallbackSourceId);

            if (defaultFallback != null)
            {
                _logger.LogInformation("Found default entry: {source}", defaultFallback.FullNamePath);

                return defaultFallback.FullIdPath;
            }
        }
        else
        {
            _logger.LogError("Could not get configuration");
        }

        _logger.LogWarning("Returning default value: {defaultValue}", defaultValue);

        return defaultValue;
    }
}
