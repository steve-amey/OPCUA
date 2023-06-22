namespace DataCollectors.ClientLibrary.Extensions;

public static class ListExtensions
{
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
    {
        return e.SelectMany(c => f(c).Flatten(f)).Concat(e);
    }
}
