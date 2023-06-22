using DataCollectors.ClientLibrary.Contracts.Enums;
using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Options;

namespace DataCollectors.ClientLibrary.Services.Alarm;

public interface IAlarmService : ITransientService
{
    Task HandleAlarmRaised(WebApi webApi, IList<NovotekAlarmDto> alarms);

    Task SendStoredItems(WebApiNames? webApiName = null);
}
