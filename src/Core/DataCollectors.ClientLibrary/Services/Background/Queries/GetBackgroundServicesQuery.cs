using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Extensions;
using DataCollectors.ClientLibrary.Services.Background.Responses;
using DataCollectors.ClientLibrary.Services.Response;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace DataCollectors.ClientLibrary.Services.Background.Queries;

public record GetBackgroundServicesQuery : IRequest<ServiceResponse<BackgroundServiceDto>>;

public class GetBackgroundServicesQueryHandler : IRequestHandler<GetBackgroundServicesQuery, ServiceResponse<BackgroundServiceDto>>
{
    private readonly IEnumerable<IHostedService> _hostedServices;
    private readonly IServiceResponseGenerator _serviceResponseGenerator;

    public GetBackgroundServicesQueryHandler(IEnumerable<IHostedService> hostedServices, IServiceResponseGenerator serviceResponseGenerator)
    {
        _hostedServices = hostedServices;
        _serviceResponseGenerator = serviceResponseGenerator;
    }

    public Task<ServiceResponse<BackgroundServiceDto>> Handle(GetBackgroundServicesQuery request, CancellationToken cancellationToken)
    {
        IList<BackgroundServiceDto> response = _hostedServices
            .OfType<IHostedServiceManager>()
            .Select(x => new BackgroundServiceDto
            {
                Name = x.ToNullSafeString().Split(".").Last(),
                Status = x.Status
            })
            .ToList();

        var result = _serviceResponseGenerator
            .CreateServiceResponse(response);

        return Task.FromResult(result);
    }
}