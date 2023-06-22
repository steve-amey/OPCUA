using System.Text.Json;
using DataCollectors.ClientLibrary.Constants;
using DataCollectors.ClientLibrary.Contracts.Enums;
using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Events.AlarmRaised;
using DataCollectors.ClientLibrary.Extensions;
using DataCollectors.ClientLibrary.Factories.Logging;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services.Http;
using DataCollectors.ClientLibrary.Services.Serialization;
using DataCollectors.ClientLibrary.Services.Storage;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataCollectors.ClientLibrary.Services.Alarm;

public class AlarmService : IAlarmService
{
    private readonly AppSettings _appSettings;

    private readonly IAlarmHttpService _alarmHttpService;
    private readonly IAlarmStorageService _alarmStorageService;
    private readonly ICustomLoggerFactory _loggerFactory;
    private readonly IPublisher _publisher;

    public AlarmService(
        IOptions<AppSettings> appSettings,
        IAlarmHttpService alarmHttpService,
        IAlarmStorageService alarmStorageService,
        ICustomLoggerFactory loggerFactory,
        IPublisher publisher)
    {
        _appSettings = appSettings.Value;
        _alarmHttpService = alarmHttpService;
        _loggerFactory = loggerFactory;
        _publisher = publisher;
        _alarmStorageService = alarmStorageService;
    }

    public async Task HandleAlarmRaised(WebApi webApi, IList<NovotekAlarmDto> alarms)
    {
        var logger = _loggerFactory.CreateLogger<AlarmService>(webApi);

        if (alarms.Any())
        {
            logger.LogInformation("Sending alarms to web api: {service}", webApi.Name);
        }

        foreach (var alarm in alarms)
        {
            var serviceResponse = await _alarmHttpService
                .Send(webApi, alarm);

            if (serviceResponse.IsSuccessStatusCode())
            {
                var alarmJson = JsonSerializer.Serialize(alarm, CustomJsonSerializerOptions.Standard);

                logger.LogDebug("Request Body: {service} - {alarm}", webApi.Name, alarmJson);

                await _alarmStorageService
                    .DeleteItem(webApi, alarm);
            }
            else
            {
                logger.LogError(JsonSerializer.Serialize(serviceResponse, CustomJsonSerializerOptions.Standard));

                try
                {
                    await _alarmStorageService
                        .StoreItem(webApi, alarm);
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Error saving item to store.");
                }
            }
        }
    }

    public async Task SendStoredItems(WebApiNames? webApiName = null)
    {
        foreach (var webApi in _appSettings.WebApi)
        {
            if (!webApi.Routes.ContainsKey(AppConstants.AddAlarmUrlKey) ||
                (webApiName != null && webApi.Name != webApiName))
            {
                continue;
            }

            var storedItemsForWebApi = await _alarmStorageService
                .GetStoredItems(webApi);

            if (!storedItemsForWebApi.Any())
            {
                continue;
            }

            var alarmRaisedEvent = new AlarmRaisedEvent
            {
                WebApi = webApi,
                AlarmInstances = storedItemsForWebApi
            };

            _ = _publisher
                .Publish(alarmRaisedEvent, CancellationToken.None);
        }
    }
}
