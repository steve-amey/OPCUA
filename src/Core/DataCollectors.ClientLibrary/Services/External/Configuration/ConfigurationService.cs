using DataCollectors.ClientLibrary.Constants;
using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services.Shared.HttpService;
using Microsoft.Extensions.Options;

namespace DataCollectors.ClientLibrary.Services.External.Configuration;

public class ConfigurationService : BaseHttpService, IConfigurationService
{
    private readonly AppSettings _appSettings;

    public ConfigurationService(IOptions<AppSettings> appSettings, IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
        _appSettings = appSettings.Value;
    }

    public async Task<ServiceResponse<string>> GetConfiguration()
    {
        var configurationWebApi = _appSettings.WebApi.Single(x => x.Name == Contracts.Enums.WebApiNames.Thingworx);

        var requestUri = configurationWebApi.Routes[AppConstants.GetConfigurationUrlKey];

        using var client = CreateHttpClient(DependencyInjection.ConfigurationServiceHttpClient, configurationWebApi);

        // Currently we are reading the configuration from Thingworx.
        // This requires the Content-Type header, but the POST request
        // does not contain a body, so that header is never set. To get
        // it to work, just pass an empty string as the body.
        var result = await PostExternal<string>(client, requestUri, new { string.Empty });
        return result;
    }
}
