namespace DataCollectors.OPCUA.Core.Application.UaClient.Responses;

public record NodeValueDto
{
    public string NodeId { get; init; } = null!;

    public DateTime ServerTimestamp { get; init; }

    public DateTime SourceTimestamp { get; init; }

    public string StatusCode { get; init; } = null!;

    public string? Value { get; init; }
}
