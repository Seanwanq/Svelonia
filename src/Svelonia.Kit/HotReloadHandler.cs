using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(Svelonia.Kit.HotReloadHandler))]

namespace Svelonia.Kit;

/// <summary>
/// Handles .NET Hot Reload events to refresh the UI.
/// </summary>
internal static class HotReloadHandler
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        // No specific cache clearing needed yet, but required by convention
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        // When code changes, reload all routers to refresh the current page
        // This will re-execute the Page constructor and apply new UI changes
        Router.ReloadAll();
    }
}
