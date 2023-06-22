using System.Net;
using System.Text.Json;
using DataCollectors.ClientLibrary.Contracts.Responses;

namespace DataCollectors.ClientLibrary.Extensions;

public static class ServiceResponseExtensions
{
    public static bool IsSuccessStatusCode(this EmptyServiceResponse serviceResponse)
    {
        var isSuccess = serviceResponse.StatusCode is >= (int)HttpStatusCode.OK and <= 299;
        return isSuccess;
    }

    public static EmptyServiceResponse ToEmptyServiceResponse<T>(this ServiceResponse<T> serviceResponse)
    {
        var serviceResponseJson = JsonSerializer.Serialize(serviceResponse);
        var result = JsonSerializer.Deserialize<EmptyServiceResponse>(serviceResponseJson);
        return result!;
    }
}
