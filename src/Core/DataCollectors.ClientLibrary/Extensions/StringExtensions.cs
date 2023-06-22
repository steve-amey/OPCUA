namespace DataCollectors.ClientLibrary.Extensions;

public static class StringExtensions
{
    public static string ReplaceLastOccurrence(this string s, string search, string replace)
    {
        var place = s.LastIndexOf(search, StringComparison.Ordinal);

        if (place == -1)
        {
            return s;
        }

        var result = s.Remove(place, search.Length).Insert(place, replace);
        return result;
    }
}
