[← Back to Svelonia.Core](./README.md)

# Debugging & Hot Reload

Svelonia provides tools to help you debug state changes and hook into the .NET Hot Reload pipeline.

## StateDebug

`StateDebug` allows you to monitor reactivity globally. This is useful for tracking down why a component is (or isn't) updating, or for logging state mutations.

### Global Change Listener

Subscribe to the `OnGlobalChange` event to receive notifications whenever *any* `State<T>` in your application changes its value.

```csharp
using Svelonia.Core;

public override void OnFrameworkInitializationCompleted()
{
#if DEBUG
    // Optional: Filter out noise (e.g., ignore internal framework states)
    StateDebug.Filter = (source) => 
    {
        // Only log states named "User..." or similar
        // (Assuming you've added some identification to your state objects)
        return source.ToString()?.Contains("User") == true;
    };

    StateDebug.OnGlobalChange += (source, newValue) =>
    {
        Console.WriteLine($"State Changed: {newValue} (Source: {source})");
    };
#endif
    
    base.OnFrameworkInitializationCompleted();
}
```

---

## Hot Reload Manager

Svelonia integrates with .NET Hot Reload (`[MetadataUpdateHandler]`). While the framework handles basic UI updates automatically, you can hook into the reload event to perform custom logic (e.g., clearing caches, resetting specific states).

### Hooking into Reloads

```csharp
using Svelonia.Core;

// In your App startup or Component
HotReloadManager.OnRequestReload += () =>
{
    Console.WriteLine("Hot Reload triggered! Refreshing custom caches...");
    MyCache.Clear();
};
```
