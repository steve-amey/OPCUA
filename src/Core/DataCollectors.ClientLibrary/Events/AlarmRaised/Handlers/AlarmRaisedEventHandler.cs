using System.Text.Json;
using DataCollectors.ClientLibrary.Factories.Logging;
using DataCollectors.ClientLibrary.Services.Alarm;
using DataCollectors.ClientLibrary.Services.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DataCollectors.ClientLibrary.Events.AlarmRaised.Handlers;

internal class AlarmRaisedEventHandler : INotificationHandler<AlarmRaisedEvent>
{
    private readonly IAlarmService _alarmService;
    private readonly ICustomLoggerFactory _loggerFactory;

    public AlarmRaisedEventHandler(IAlarmService alarmService, ICustomLoggerFactory loggerFactory)
    {
        _alarmService = alarmService;
        _loggerFactory = loggerFactory;
    }

    public async Task Handle(AlarmRaisedEvent notification, CancellationToken cancellationToken)
    {
        _loggerFactory
            .CreateLogger<AlarmRaisedEventHandler>(notification.WebApi)
            .LogInformation("Handling alarms: {alarm}", JsonSerializer.Serialize(notification.AlarmInstances, CustomJsonSerializerOptions.Standard));

        await _alarmService
            .HandleAlarmRaised(notification.WebApi, notification.AlarmInstances);
    }
}
