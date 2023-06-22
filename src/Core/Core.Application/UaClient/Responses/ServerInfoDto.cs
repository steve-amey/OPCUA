namespace DataCollectors.OPCUA.Core.Application.UaClient.Responses;

public record ServerInfoDto
{
    public DateTime StartTime { get; init; }

    public DateTime CurrentTime { get; init; }

    public string State { get; init; } = null!;

    public string ProductUri { get; init; } = null!;

    public string ManufacturerName { get; init; } = null!;

    public string ProductName { get; init; } = null!;

    public string SoftwareVersion { get; init; } = null!;

    public string BuildNumber { get; init; } = null!;

    public DateTime BuildDate { get; init; }
}
