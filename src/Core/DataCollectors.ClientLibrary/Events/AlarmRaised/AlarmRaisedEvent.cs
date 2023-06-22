using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Options;
using MediatR;

namespace DataCollectors.ClientLibrary.Events.AlarmRaised;

public record AlarmRaisedEvent : INotification
{
    public WebApi WebApi { get; init; } = null!;

    public IList<NovotekAlarmDto> AlarmInstances { get; init; } = new List<NovotekAlarmDto>();
}