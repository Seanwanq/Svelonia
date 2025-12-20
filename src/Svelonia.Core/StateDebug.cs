namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public static class StateDebug
{
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
        OnGlobalChange?.Invoke(source, newValue);
    }
}