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
}
