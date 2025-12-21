namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public static class StateExtensions
{
    /// <summary>
    /// Creates a Computed property based on the source state.
    /// </summary>
    public static Computed<TResult> Select<TSource, TResult>(this State<TSource> source, Func<TSource, TResult> selector)
    {
        return new Computed<TResult>(() => selector(source.Value));
    }

    /// <summary>
    /// Fluently branch based on state value
    /// </summary>
    public static Computed<TResult> Match<TSource, TResult>(this State<TSource> source, params (TSource value, TResult result)[] cases)
        where TSource : notnull
    {
        return new Computed<TResult>(() =>
        {
            var current = source.Value;
            foreach (var (val, res) in cases)
            {
                if (Equals(current, val)) return res;
            }
            return default!;
        });
    }

    /// <summary>
    /// Fluently branch based on state value with default
    /// </summary>
    public static Computed<TResult> Match<TSource, TResult>(this State<TSource> source, TResult @default, params (TSource value, TResult result)[] cases)
        where TSource : notnull
    {
        return new Computed<TResult>(() =>
        {
            var current = source.Value;
            foreach (var (val, res) in cases)
            {
                if (Equals(current, val)) return res;
            }
            return @default;
        });
    }
}
