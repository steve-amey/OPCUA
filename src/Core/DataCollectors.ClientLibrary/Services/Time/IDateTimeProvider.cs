namespace DataCollectors.ClientLibrary.Services.Time;

public interface IDateTimeProvider : ITransientService
{
    DateTime Now { get; }

    DateTime UtcNow { get; }
}
