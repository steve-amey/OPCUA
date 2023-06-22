namespace DataCollectors.ClientLibrary.Extensions;

public static class DateTimeExtensions
{
    public static string ToReadableString(this TimeSpan timeSpan, bool includeMilliseconds = true)
    {
        var formatted =
            $"{(timeSpan.Days > 0 ? $"{timeSpan.Days:0} days, " : string.Empty)}" +
            $"{(timeSpan.Hours > 0 ? $"{timeSpan.Hours:0} hours, " : string.Empty)}" +
            $"{(timeSpan.Minutes > 0 ? $"{timeSpan.Minutes:0} minutes, " : string.Empty)}" +
            $"{(timeSpan.Minutes > 0 && timeSpan.Seconds > 0 ? $"{timeSpan.Seconds:0} seconds " : string.Empty)}" +
            $"{(timeSpan.Minutes == 0 && timeSpan.Seconds > 0 ? $"{timeSpan.Seconds:0}.{Math.Floor((decimal)timeSpan.Milliseconds / 100):00} seconds " : string.Empty)}" +
            $"{(includeMilliseconds && timeSpan.Seconds == 0 ? $"{timeSpan.Milliseconds:0} millisecond(s)" : string.Empty)}";

        formatted = formatted.Trim();

        if (formatted.EndsWith(","))
        {
            formatted = formatted.ReplaceLastOccurrence(",", string.Empty);
        }

        return formatted;
    }
}
