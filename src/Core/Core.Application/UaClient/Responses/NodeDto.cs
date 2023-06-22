namespace DataCollectors.OPCUA.Core.Application.UaClient.Responses;

public record NodeDto
{
    public string NodeId { get; init; } = null!;

    public string DisplayName { get; init; } = null!;

    public string NodeClass { get; init; } = null!;
}
