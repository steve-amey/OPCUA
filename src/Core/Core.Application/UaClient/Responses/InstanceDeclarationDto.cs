namespace DataCollectors.OPCUA.Core.Application.UaClient.Responses;

public record InstanceDeclarationDto
{
    public string RootTypeId { get; init; } = null!;

    public IList<string> BrowsePath { get; init; } = new List<string>();

    public string BrowsePathDisplayText { get; init; } = null!;

    public string DisplayPath { get; init; } = null!;

    public string NodeId { get; init; } = null!;

    public string NodeClass { get; init; } = null!;

    public string BrowseName { get; init; } = null!;

    public string DisplayName { get; init; } = null!;

    public string Description { get; init; } = null!;

    public string? ModellingRule { get; init; }

    public string DataType { get; init; } = null!;

    public int ValueRank { get; init; }

    public string BuiltInType { get; init; } = null!;

    public string DataTypeDisplayText { get; init; } = null!;

    public InstanceDeclarationDto? OverriddenDeclaration { get; init; } = null!;
}
