using System.Net;
using DataCollectors.ClientLibrary.Contracts.Responses;

namespace DataCollectors.ClientLibrary.Services.Response;

public interface IServiceResponseGenerator : ITransientService
{
    ServiceResponse<TResponse> CreateServiceResponse<TResponse>(IList<TResponse> response, HttpStatusCode statusCode = HttpStatusCode.OK);

    ServiceResponse<TResponse> CreateServiceResponse<TResponse>(TResponse? response, HttpStatusCode statusCode = HttpStatusCode.OK);

    EmptyServiceResponse CreateNotFoundServiceResponse(string message);

    EmptyServiceResponse CreateEmptyServiceResponse();

    EmptyServiceResponse CreateExceptionResponse(Exception exception);
}
