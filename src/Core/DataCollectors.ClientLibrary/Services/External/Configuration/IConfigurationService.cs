using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Services.External.Configuration.Responses;

namespace DataCollectors.ClientLibrary.Services.External.Configuration;

public interface IConfigurationService : ITransientService
{
    Task<ServiceResponse<string>> GetConfiguration();
}
