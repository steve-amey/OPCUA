using System.Text.Json.Serialization;

namespace DataCollectors.ClientLibrary.Services.Configuration.Models;

public record EnterpriseModel
{
    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; init; } = null!;

    public string Name { get; init; } = null!;

    public IList<Site> Sites { get; init; } = new List<Site>();
}

public record Site
{
    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; init; } = null!;

    public string Name { get; init; } = null!;

    public IList<Area> Areas { get; init; } = new List<Area>();
}

public record Area
{
    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; init; } = null!;

    public string Name { get; init; } = null!;

    public IList<WorkCenter> WorkCenters { get; init; } = new List<WorkCenter>();
}

public record WorkCenter
{
    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; init; } = null!;

    public string Name { get; init; } = null!;

    public IList<WorkUnit> WorkUnits { get; init; } = new List<WorkUnit>();
}

public record WorkUnit
{
    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; init; } = null!;

    public string Name { get; init; } = null!;

    public IList<WorkCell> WorkCells { get; init; } = new List<WorkCell>();
}

public record WorkCell
{
    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; init; } = null!;

    public string Name { get; init; } = null!;
}