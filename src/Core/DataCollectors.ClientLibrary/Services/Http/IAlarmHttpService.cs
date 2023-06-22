using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Options;

namespace DataCollectors.ClientLibrary.Services.Http;

public interface IAlarmHttpService : ITransientService
{
    Task<EmptyServiceResponse> Send(WebApi webApi, NovotekAlarmDto alarm);
}
