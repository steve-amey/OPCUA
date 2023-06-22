namespace DataCollectors.ClientLibrary.Services.Time;

internal class DateTimeProvider : IDateTimeProvider
{
    public virtual DateTime Now => DateTime.Now;

    public virtual DateTime UtcNow => DateTime.UtcNow;
}
