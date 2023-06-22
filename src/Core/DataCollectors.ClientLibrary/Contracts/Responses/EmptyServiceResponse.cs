namespace DataCollectors.ClientLibrary.Contracts.Responses;

public record EmptyServiceResponse
{
    public int StatusCode { get; init; }

    public IList<string>? Errors { get; init; }
}
