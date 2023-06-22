using DataCollectors.OPCUA.Core.Application.UaClient.Responses;
using DataCollectors.OPCUA.Core.Application.UaClient.Services;
using MediatR;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Queries;

public record GetNodeValueQuery : IRequest<NodeValueDto>
{
    public string NodeId { get; init; } = null!;
}

internal class GetNodeValueQueryHandler : IRequestHandler<GetNodeValueQuery, NodeValueDto>
{
    private readonly IUaClientService _uaClientService;

    public GetNodeValueQueryHandler(IUaClientService uaClientService)
    {
        _uaClientService = uaClientService;
    }

    public Task<NodeValueDto> Handle(GetNodeValueQuery query, CancellationToken cancellationToken)
    {
        if (!_uaClientService.IsConnected)
        {
            throw new Exception("UA Client not connected");
        }

        var dataValue = _uaClientService.GetNodeValue(query.NodeId);

        if (dataValue == null)
        {
            throw new Exception($"Node not found: {query.NodeId}");
        }

        var result = new NodeValueDto
        {
            NodeId = query.NodeId,
            ServerTimestamp = dataValue.ServerTimestamp,
            SourceTimestamp = dataValue.SourceTimestamp,
            StatusCode = dataValue.StatusCode.ToString(),
            Value = dataValue.Value.ToString()
        };

        return Task.FromResult(result);
    }
}