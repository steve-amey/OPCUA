namespace DataCollectors.ClientLibrary.Services.Configuration.Responses;

public record ConfigurationDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Type { get; init; } = null!;

    public string FullNamePath { get; init; } = null!;

    public string FullIdPath { get; init; } = null!;

    public IList<ConfigurationDto> Items { get; init; } = new List<ConfigurationDto>();
}
