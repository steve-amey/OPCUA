using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Events.AlarmRaised;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services.Shared.HttpService;

namespace DataCollectors.ClientLibrary.Services.Http;

internal class AlarmHttpService : BaseHttpService, IAlarmHttpService
{
    public AlarmHttpService(IHttpClientFactory httpClientFactory)
        : base(httpClientFactory)
    {
    }

    public async Task<EmptyServiceResponse> Send(WebApi webApi, NovotekAlarmDto alarm)
    {
        using var client = CreateHttpClient(DependencyInjection.WebApiHttpClient, webApi);

        var requestUri = $"{webApi.UrlPrefix}{webApi.Routes[Constants.AppConstants.AddAlarmUrlKey]}";

        var result = await Post<AlarmRaisedEvent>(client, requestUri, alarm);
        return result;
    }
}