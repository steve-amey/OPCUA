using System.Text.Json;
using DataCollectors.ClientLibrary.Services.Configuration.Models;
using DataCollectors.ClientLibrary.Services.Configuration.Responses;
using DataCollectors.ClientLibrary.Services.Serialization;
using Microsoft.Extensions.Caching.Memory;

namespace DataCollectors.ClientLibrary.Services.Cache;

public class CacheService : ICacheService
{
    private const string ConfigurationCacheKey = "Configuration";

    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<ConfigurationDto?> GetConfiguration(CancellationToken cancellationToken)
    {
        var cacheJson = _memoryCache.Get<string>(ConfigurationCacheKey);

        var cacheModel = !string.IsNullOrWhiteSpace(cacheJson) ?
            JsonSerializer.Deserialize<ConfigurationDto>(cacheJson)
            : null;

        return Task.FromResult(cacheModel);
    }

    public Task<ConfigurationDto?> UpdateConfiguration(string value, CancellationToken cancellationToken)
    {
        ConfigurationDto? result = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.FromResult(result);
        }

        var enterpriseModel = JsonSerializer.Deserialize<EnterpriseModel>(value, CustomJsonSerializerOptions.StandardDeserialise);

        if (enterpriseModel == null)
        {
            return Task.FromResult(result);
        }

        result = CreateConfigurationDto(null, "Enterprise", enterpriseModel.Name, enterpriseModel.FriendlyName);

        foreach (var site in enterpriseModel.Sites)
        {
            AddSite(result, site);
        }

        var hierarchyJson = JsonSerializer.Serialize(result);

        _memoryCache.Set(ConfigurationCacheKey, hierarchyJson);

        return Task.FromResult(result)!;
    }

    private static void AddSite(ConfigurationDto parent, Site site)
    {
        var model = CreateConfigurationDto(parent, nameof(Site), site.Name, site.FriendlyName);

        foreach (var area in site.Areas)
        {
            AddArea(model, area);
        }

        parent.Items.Add(model);
    }

    private static void AddArea(ConfigurationDto parent, Area area)
    {
        var model = CreateConfigurationDto(parent, nameof(Area), area.Name, area.FriendlyName);

        foreach (var workCenter in area.WorkCenters)
        {
            AddWorkCentre(model, workCenter);
        }

        parent.Items.Add(model);
    }

    private static void AddWorkCentre(ConfigurationDto parent, WorkCenter workCenter)
    {
        var model = CreateConfigurationDto(parent, nameof(WorkCenter), workCenter.Name, workCenter.FriendlyName);

        foreach (var workUnit in workCenter.WorkUnits)
        {
            AddWorkUnit(model, workUnit);
        }

        parent.Items.Add(model);
    }

    private static void AddWorkUnit(ConfigurationDto parent, WorkUnit workUnit)
    {
        var model = CreateConfigurationDto(parent, nameof(WorkUnit), workUnit.Name, workUnit.FriendlyName);

        foreach (var workCell in workUnit.WorkCells)
        {
            AddWorkCell(model, workCell);
        }

        parent.Items.Add(model);
    }

    private static void AddWorkCell(ConfigurationDto parent, WorkCell workCell)
    {
        var model = CreateConfigurationDto(parent, nameof(WorkCell), workCell.Name, workCell.FriendlyName);

        parent.Items.Add(model);
    }

    private static ConfigurationDto CreateConfigurationDto(ConfigurationDto? parent, string type, string name, string friendlyName)
    {
        var model = new ConfigurationDto
        {
            Id = Guid.Parse(name),
            Name = friendlyName,
            Type = type,
            FullIdPath = parent != null ? $"{parent.FullIdPath}.{name}" : name,
            FullNamePath = parent != null ? $"{parent.FullNamePath}.{friendlyName}" : friendlyName
        };

        return model;
    }
}
