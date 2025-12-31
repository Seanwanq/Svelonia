# Ahead-of-Time (AOT) Compilation

Svelonia is designed to be compatible with Native AOT and Trimming. To achieve this, the framework avoids runtime reflection in critical paths (like service resolution and JSON parsing).

## Implementing `ISveloniaApplication`

For framework features like keyboard shortcuts (`OnKey`) or Mediator commands to work in an AOT environment, your main `Application` class must implement the `ISveloniaApplication` interface. This provides a type-safe way for the framework to access your `IServiceProvider`.

### Example `App.cs`

```csharp
using Svelonia.Core;

public class App : Application, ISveloniaApplication
{
    // Required by ISveloniaApplication
    public IServiceProvider? Services { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        // ... register your services
        Services = collection.BuildServiceProvider();
        
        base.OnFrameworkInitializationCompleted();
    }
}
```

## JSON Theming

SveloniaTheme uses source-generated JSON contexts. If you define custom theme objects, ensure you use the `JsonSerializerContext` pattern to avoid reflection-based deserialization.

## Mediator Registry

Always use `services.AddSveloniaDataAot()` in your DI setup. This uses the generated static registry instead of scanning assemblies at runtime.
