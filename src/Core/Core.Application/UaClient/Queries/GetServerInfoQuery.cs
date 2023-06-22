using DataCollectors.OPCUA.Core.Application.UaClient.Responses;
using DataCollectors.OPCUA.Core.Application.UaClient.Services;
using MediatR;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Queries;

public record GetServerInfoQuery : IRequest<ServerInfoDto>;

internal class GetServerInfoQueryHandler : IRequestHandler<GetServerInfoQuery, ServerInfoDto>
{
    private readonly IUaClientService _uaClientService;

    public GetServerInfoQueryHandler(IUaClientService uaClientService)
    {
        _uaClientService = uaClientService;
    }

    public Task<ServerInfoDto> Handle(GetServerInfoQuery query, CancellationToken cancellationToken)
    {
        if (!_uaClientService.IsConnected)
        {
            throw new Exception("UA Client not connected");
        }

        var serverStatus = _uaClientService.GetServerStatus();

        var result = new ServerInfoDto
        {
            StartTime = serverStatus.StartTime,
            CurrentTime = serverStatus.CurrentTime,
            State = serverStatus.State.ToString(),
            BuildNumber = serverStatus.BuildInfo.BuildNumber,
            ManufacturerName = serverStatus.BuildInfo.ManufacturerName,
            ProductName = serverStatus.BuildInfo.ProductName,
            ProductUri = serverStatus.BuildInfo.ProductUri,
            SoftwareVersion = serverStatus.BuildInfo.SoftwareVersion,
            BuildDate = serverStatus.BuildInfo.BuildDate
        };

        return Task.FromResult(result);
    }
}