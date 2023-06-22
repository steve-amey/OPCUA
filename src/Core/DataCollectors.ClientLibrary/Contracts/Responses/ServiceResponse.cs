namespace DataCollectors.ClientLibrary.Contracts.Responses;

public record ServiceResponse<T> : EmptyServiceResponse
{
    public IList<T>? Result { get; init; }
}