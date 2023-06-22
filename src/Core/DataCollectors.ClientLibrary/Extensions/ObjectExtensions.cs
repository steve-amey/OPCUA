namespace DataCollectors.ClientLibrary.Extensions;

public static class ObjectExtensions
{
    public static string ToNullSafeString(this object? obj, string defaultValue = "")
    {
        return obj?.ToString()!.Trim() ?? defaultValue;
    }

    public static string? ToNullableString(this object? obj)
    {
        return string.IsNullOrWhiteSpace(obj?.ToString()) ? null : obj.ToString()!.Trim();
    }
}
