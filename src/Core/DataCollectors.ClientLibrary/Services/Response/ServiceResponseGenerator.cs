using System.Net;
using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Exceptions;

namespace DataCollectors.ClientLibrary.Services.Response;

public class ServiceResponseGenerator : IServiceResponseGenerator
{
    public ServiceResponse<TResponse> CreateServiceResponse<TResponse>(IList<TResponse> response, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var result = new ServiceResponse<TResponse>
        {
            StatusCode = (int)statusCode,
            Result = response
        };

        return result;
    }

    public ServiceResponse<TResponse> CreateServiceResponse<TResponse>(TResponse? response, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var result = new ServiceResponse<TResponse>
        {
            StatusCode = (int)statusCode,
            Result = response != null ? new List<TResponse> { response } : null
        };

        return result;
    }

    public EmptyServiceResponse CreateNotFoundServiceResponse(string message)
    {
        var response = new EmptyServiceResponse
        {
            StatusCode = (int)HttpStatusCode.NotFound,
            Errors = new List<string> { message }
        };

        return response;
    }

    public EmptyServiceResponse CreateEmptyServiceResponse()
    {
        var serviceResponse = new EmptyServiceResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };

        return serviceResponse;
    }

    public EmptyServiceResponse CreateExceptionResponse(Exception exception)
    {
        var statusCode = exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            ValidationException => HttpStatusCode.UnprocessableEntity,
            _ => HttpStatusCode.InternalServerError
        };

        var message = exception.GetBaseException().Message;
        var errors = new List<string>();
        var statusCodeValue = (int)statusCode;

        switch (exception)
        {
            case ValidationException validationException:

                foreach (var item in validationException.Errors)
                {
                    errors.Add($"{item.Key} - {item.Value}");
                }

                break;
            default:
                errors.Add(message);
                break;
        }

        var response = new EmptyServiceResponse
        {
            StatusCode = statusCodeValue,
            Errors = errors
        };

        return response;
    }
}