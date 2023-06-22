using DataCollectors.OPCUA.Core.Application.UaClient.Responses;
using DataCollectors.OPCUA.Core.Application.UaClient.Services;
using MediatR;
using Opc.Ua;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Queries;

public record BrowseNodeQuery : IRequest<IList<NodeDto>>
{
    public string NodeId { get; init; }
}

internal class BrowseNodeQueryHandler : IRequestHandler<BrowseNodeQuery, IList<NodeDto>>
{
    private readonly IUaClientService _uaClientService;

    public BrowseNodeQueryHandler(IUaClientService uaClientService)
    {
        _uaClientService = uaClientService;
    }

    public Task<IList<NodeDto>> Handle(BrowseNodeQuery query, CancellationToken cancellationToken)
    {
        if (!_uaClientService.IsConnected)
        {
            throw new Exception("UA Client not connected");
        }

        IList<NodeDto> result = new List<NodeDto>();

        var nodeId = BuildNodeIdFromQuery(query);
        var browseResults = _uaClientService.BrowseNode(nodeId!);

        foreach (var referenceDescription in browseResults)
        {
            result.Add(new NodeDto
            {
                NodeId = referenceDescription.NodeId.ToString()!,
                DisplayName = referenceDescription.DisplayName.Text,
                NodeClass = referenceDescription.NodeClass.ToString()
            });
        }

        return Task.FromResult(result);
    }

    private static NodeId? BuildNodeIdFromQuery(BrowseNodeQuery query)
    {
        NodeId? result;

        switch (query.NodeId)
        {
            case "Root":
                result = ObjectIds.RootFolder;
                break;
            case "Server":
                result = ObjectIds.Server;
                break;
            default:

                result = uint.TryParse(query.NodeId, out var i) ? new NodeId(i) : NodeId.Parse(query.NodeId);
                break;
        }

        return result;
    }
}