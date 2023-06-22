using DataCollectors.OPCUA.Core.Application.UaClient.Helpers;
using DataCollectors.OPCUA.Core.Application.UaClient.Responses;
using DataCollectors.OPCUA.Core.Application.UaClient.Services;
using MediatR;
using Opc.Ua;

namespace DataCollectors.OPCUA.Core.Application.UaClient.Queries;

public record GetFieldsQuery : IRequest<IList<InstanceDeclarationDto>>
{
    public string NodeId { get; init; } = null!;
}

internal class GetFieldsQueryHandler : IRequestHandler<GetFieldsQuery, IList<InstanceDeclarationDto>>
{
    private readonly IUaClientService _uaClientService;

    public GetFieldsQueryHandler(IUaClientService uaClientService)
    {
        _uaClientService = uaClientService;
    }

    public Task<IList<InstanceDeclarationDto>> Handle(GetFieldsQuery query, CancellationToken cancellationToken)
    {
        if (!_uaClientService.IsConnected)
        {
            throw new Exception("UA Client not connected");
        }

        var node = NodeId.Parse(query.NodeId);

        var instanceDeclarations = _uaClientService.CreateInstanceDeclarationsForType(node);

        IList<InstanceDeclarationDto> result = new List<InstanceDeclarationDto>();

        foreach (var instanceDeclaration in instanceDeclarations)
        {
            result.Add(CreateInstanceDeclarationDtoFromInstanceDeclaration(instanceDeclaration));
        }

        return Task.FromResult(result);
    }

    private InstanceDeclarationDto CreateInstanceDeclarationDtoFromInstanceDeclaration(InstanceDeclaration instanceDeclaration)
    {
        var result = new InstanceDeclarationDto
        {
            RootTypeId = instanceDeclaration.RootTypeId.ToString(),
            BrowsePath = instanceDeclaration.BrowsePath.Select(x => x.ToString()).ToList(),
            BrowsePathDisplayText = instanceDeclaration.BrowsePathDisplayText,
            DisplayPath = instanceDeclaration.DisplayPath,
            NodeId = instanceDeclaration.NodeId.ToString(),
            NodeClass = instanceDeclaration.NodeClass.ToString(),
            BrowseName = instanceDeclaration.BrowseName.ToString(),
            DisplayName = instanceDeclaration.DisplayName,
            Description = instanceDeclaration.Description,
            ModellingRule = instanceDeclaration.ModellingRule != null ? instanceDeclaration.ModellingRule.ToString() : null,
            DataType = instanceDeclaration.DataType.ToString(),
            ValueRank = instanceDeclaration.ValueRank,
            BuiltInType = instanceDeclaration.BuiltInType.ToString(),
            DataTypeDisplayText = instanceDeclaration.DataTypeDisplayText,
            OverriddenDeclaration = instanceDeclaration.OverriddenDeclaration != null ?
                CreateInstanceDeclarationDtoFromInstanceDeclaration(instanceDeclaration.OverriddenDeclaration)
                : null
        };

        return result;
    }
}