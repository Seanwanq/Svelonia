using Svelonia.Core;

namespace Svelonia.DevTools;

/// <summary>
/// 
/// </summary>
public class LogEntry
{
    /// <summary>
    /// 
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.Now;

    /// <summary>
    /// 
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string Value { get; set; } = "";
}

/// <summary>
/// 
/// </summary>
public class DevToolsContext
{
    /// <summary>
    /// 
    /// </summary>
    public static DevToolsContext Instance { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    public StateList<LogEntry> Logs { get; } = new();

    /// <summary>
    /// 
    /// </summary>
    public State<bool> IsEnabled { get; } = new(false);

    /// <summary>
    /// 
    /// </summary>
    public void Enable()
    {
        if (IsEnabled.Value) return;
        IsEnabled.Value = true;

        StateDebug.OnGlobalChange += OnStateChanged;
    }

    private void OnStateChanged(object source, object? newValue)
    {
        // Simple logging
        var type = source.GetType();
        var genericType = type.IsGenericType ? type.GetGenericArguments()[0].Name : "Unknown";

        // Try to get DebugName via reflection
        var nameProp = type.GetProperty("DebugName");
        var name = nameProp?.GetValue(source) as string;

        var sourceName = !string.IsNullOrEmpty(name)
            ? $"{name} (State<{genericType}>)"
            : $"State<{genericType}>";

        Logs.Insert(0, new LogEntry
        {
            Source = sourceName,
            Value = newValue?.ToString() ?? "null"
        });

        // Limit log size
        if (Logs.Count > 100) Logs.RemoveAt(Logs.Count - 1);
    }
}
