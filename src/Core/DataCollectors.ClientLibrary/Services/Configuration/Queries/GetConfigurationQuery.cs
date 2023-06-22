using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Exceptions;
using DataCollectors.ClientLibrary.Services.Configuration.Accessors;
using DataCollectors.ClientLibrary.Services.Configuration.Responses;
using DataCollectors.ClientLibrary.Services.Response;
using MediatR;

namespace DataCollectors.ClientLibrary.Services.Configuration.Queries;

public record GetConfigurationQuery : IRequest<ServiceResponse<ConfigurationDto>>;

public class GetConfigurationQueryHandler : IRequestHandler<GetConfigurationQuery, ServiceResponse<ConfigurationDto>>
{
    private readonly IConfigurationAccessor _configurationAccessor;
    private readonly IServiceResponseGenerator _serviceResponseGenerator;

    public GetConfigurationQueryHandler(IConfigurationAccessor configurationAccessor, IServiceResponseGenerator serviceResponseGenerator)
    {
        _configurationAccessor = configurationAccessor;
        _serviceResponseGenerator = serviceResponseGenerator;
    }

    public async Task<ServiceResponse<ConfigurationDto>> Handle(GetConfigurationQuery request, CancellationToken cancellationToken)
    {
        var response = await _configurationAccessor.GetConfiguration(cancellationToken)
                       ?? throw new NotFoundException("Configuration not found");

        var result = _serviceResponseGenerator
            .CreateServiceResponse(response);

        return result;
    }
}