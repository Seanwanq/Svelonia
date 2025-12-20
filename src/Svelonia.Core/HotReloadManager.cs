using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(Svelonia.Core.HotReloadHandler))]

namespace Svelonia.Core;

/// <summary>
/// Global manager for Hot Reload events in Svelonia.
/// </summary>
public static class HotReloadManager
{
    /// <summary>
    /// Occurs when the application code is updated via Hot Reload.
    /// </summary>
    public static event Action? OnRequestReload;

    /// <summary>
    /// Triggers the reload event.
    /// </summary>
    internal static void TriggerReload()
    {
        OnRequestReload?.Invoke();
    }
}

/// <summary>
/// Internal handler for the .NET MetadataUpdateHandler attribute.
/// </summary>
internal static class HotReloadHandler
{
    public static void ClearCache(Type[]? updatedTypes) { }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        HotReloadManager.TriggerReload();
    }
}
