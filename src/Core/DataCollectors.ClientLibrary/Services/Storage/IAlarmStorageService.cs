using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Options;

namespace DataCollectors.ClientLibrary.Services.Storage;

public interface IAlarmStorageService : ITransientService
{
    Task<IList<NovotekAlarmDto>> GetStoredItems(WebApi webApi);

    Task StoreItem(WebApi webApi, NovotekAlarmDto alarm);

    Task DeleteItem(WebApi webApi, NovotekAlarmDto alarm);
}
