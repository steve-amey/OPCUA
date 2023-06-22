using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Services.Response;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace DataCollectors.ClientLibrary.Services.Background.Commands;

public record StartBackgroundServiceCommand : IRequest<EmptyServiceResponse>
{
    public string Name { get; init; } = null!;
}

public class StartBackgroundServiceCommandHandler : IRequestHandler<StartBackgroundServiceCommand, EmptyServiceResponse>
{
    private readonly IEnumerable<IHostedService> _hostedServices;
    private readonly IServiceResponseGenerator _serviceResponseGenerator;

    public StartBackgroundServiceCommandHandler(IEnumerable<IHostedService> hostedServices, IServiceResponseGenerator serviceResponseGenerator)
    {
        _hostedServices = hostedServices;
        _serviceResponseGenerator = serviceResponseGenerator;
    }

    public async Task<EmptyServiceResponse> Handle(StartBackgroundServiceCommand request, CancellationToken cancellationToken)
    {
        if (_hostedServices.FirstOrDefault(x => x.ToString()!.EndsWith(request.Name)) is IHostedServiceManager hostedService)
        {
            await hostedService.Start(cancellationToken);
        }

        var result = _serviceResponseGenerator
            .CreateEmptyServiceResponse();

        return result;
    }
}