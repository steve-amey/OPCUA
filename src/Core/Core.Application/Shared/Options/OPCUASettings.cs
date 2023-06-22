namespace DataCollectors.OPCUA.Core.Application.Shared.Options;

public record OPCUASettings
{
    public string ServerUrl { get; init; } = null!;

    public IList<SubscriptionOption> Subscriptions { get; init; } = new List<SubscriptionOption>();

    public IList<string> Fields { get; init; } = new List<string>();
}

public record SubscriptionOption
{
    public string Name { get; init; } = null!;
}