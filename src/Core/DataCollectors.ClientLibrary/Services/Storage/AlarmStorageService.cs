using System.Collections.Concurrent;
using System.Text.Json;
using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataCollectors.ClientLibrary.Services.Storage;

public class AlarmStorageService : IAlarmStorageService
{
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;

    public AlarmStorageService(IOptions<AppSettings> appSettings, ILogger<AlarmStorageService> logger)
    {
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public Task<IList<NovotekAlarmDto>> GetStoredItems(WebApi webApi)
    {
        var path = Path.Combine(_appSettings.CacheLocation, webApi.Name.ToString());

        if (!Directory.Exists(path))
        {
            return Task.FromResult((IList<NovotekAlarmDto>)new List<NovotekAlarmDto>());
        }

        var list = new ConcurrentBag<string>();

        Parallel.ForEach(Directory.EnumerateFiles(path, "*.json"), (file) =>
        {
            list.Add(File.ReadAllText(file));
        });

        var stringConvertedToJsonArray = $"[{string.Join(",", list.ToList())}]";

        var result = JsonSerializer.Deserialize<IList<NovotekAlarmDto>>(stringConvertedToJsonArray, CustomJsonSerializerOptions.StandardDeserialise);

        return Task.FromResult(result)!;
    }

    public Task StoreItem(WebApi webApi, NovotekAlarmDto alarm)
    {
        var storageLocation = Path.Combine(_appSettings.CacheLocation, webApi.Name.ToString(), GetFileName(alarm));

        var path = Path.GetDirectoryName(storageLocation);

        if (!string.IsNullOrWhiteSpace(path) &&
            !Directory.Exists(path))
        {
            _logger.LogInformation("Creating directory: {path}", path);

            Directory.CreateDirectory(path);
        }

        if (!File.Exists(storageLocation))
        {
            var alarmJson = JsonSerializer.Serialize(alarm, CustomJsonSerializerOptions.Indented);

            File.WriteAllText(storageLocation, alarmJson);
        }

        return Task.CompletedTask;
    }

    public Task DeleteItem(WebApi webApi, NovotekAlarmDto alarm)
    {
        var storageLocation = Path.Combine(_appSettings.CacheLocation, webApi.Name.ToString(), GetFileName(alarm));

        if (File.Exists(storageLocation))
        {
            File.Delete(storageLocation);
        }

        return Task.CompletedTask;
    }

    private static string GetFileName(NovotekAlarmDto alarm)
    {
        var result = $"ALM_{alarm.Id}.json";
        return result;
    }
}
