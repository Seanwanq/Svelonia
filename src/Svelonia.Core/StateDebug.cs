namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public static class StateDebug
{
    /// <summary>
    /// Optional filter to reduce noise. If set, only sources returning true will trigger the event.
    /// </summary>
    public static Predicate<object>? Filter { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static event Action<object, object?>? OnGlobalChange;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="newValue"></param>
    public static void NotifyChange(object source, object? newValue)
    {
        if (Filter != null && !Filter(source)) return;
        OnGlobalChange?.Invoke(source, newValue);
    }
}