namespace DataCollectors.ClientLibrary.Contracts.Responses;

public record NovotekAlarmDto
{
    public Guid Id { get; init; }

    public string SourceAlarmId { get; init; } = null!;

    public int PriorityId { get; init; }

    public string? StartTime { get; init; } = null!;

    public string? EndTime { get; init; }

    public string Source { get; init; } = null!;

    public string SourceProperty { get; init; } = null!;

    public string? Type { get; init; }

    public string? AlertName { get; init; }

    public string? PropertyDescription { get; init; }

    public string SourceSystemId { get; init; } = null!;

    public string Message { get; init; } = null!;
}
