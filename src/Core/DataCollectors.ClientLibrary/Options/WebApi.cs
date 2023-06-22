using DataCollectors.ClientLibrary.Contracts.Enums;

namespace DataCollectors.ClientLibrary.Options;

public record WebApi
{
    public WebApiNames Name { get; init; }

    public string BaseUrl { get; init; } = null!;

    public string? UrlPrefix { get; init; }

    public IDictionary<string, string> Routes { get; init; } = new Dictionary<string, string>();

    public IDictionary<string, string>? Headers { get; init; }
}