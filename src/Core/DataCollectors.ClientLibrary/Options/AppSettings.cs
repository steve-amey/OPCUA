using DataCollectors.ClientLibrary.Contracts.Enums;

namespace DataCollectors.ClientLibrary.Options;

public record AppSettings
{
    public string CacheLocation { get; init; } = null!;

    public int Retry { get; init; }

    public string FallbackSourceId { get; init; } = null!;

    public string FallbackSourceSystemId { get; init; } = null!;

    public TimeSpan ScheduleInterval { get; init; }

    public IList<WebApiNames> UpdateConfigurationWebApis { get; init; } = new List<WebApiNames>();

    public IList<WebApi> WebApi { get; init; } = new List<WebApi>();
}
