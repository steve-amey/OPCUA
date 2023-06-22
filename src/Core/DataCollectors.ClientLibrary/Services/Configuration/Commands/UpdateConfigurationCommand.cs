using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Services.Cache;
using DataCollectors.ClientLibrary.Services.Response;
using MediatR;

namespace DataCollectors.ClientLibrary.Services.Configuration.Commands;

public record UpdateConfigurationCommand : IRequest<EmptyServiceResponse>
{
    public string Value { get; init; } = null!;
}

internal class UpdateConfigurationCommandHandler : IRequestHandler<UpdateConfigurationCommand, EmptyServiceResponse>
{
    private readonly ICacheService _cacheService;
    private readonly IServiceResponseGenerator _serviceResponseGenerator;

    public UpdateConfigurationCommandHandler(ICacheService cacheService, IServiceResponseGenerator serviceResponseGenerator)
    {
        _cacheService = cacheService;
        _serviceResponseGenerator = serviceResponseGenerator;
    }

    public Task<EmptyServiceResponse> Handle(UpdateConfigurationCommand request, CancellationToken cancellationToken)
    {
        _cacheService
            .UpdateConfiguration(request.Value, cancellationToken);

        var response = _serviceResponseGenerator
            .CreateEmptyServiceResponse();

        return Task.FromResult(response);
    }
}