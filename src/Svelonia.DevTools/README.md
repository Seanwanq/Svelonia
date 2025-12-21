# Svelonia.DevTools

Debugging tools for Svelonia applications.

## Features

*   **State Inspector**: View and modify active `State<T>` values in runtime.
*   **Component Tree**: Visualize the hierarchy of Svelonia components.

## Usage

Enable DevTools in your `App.cs` (usually within a `#if DEBUG` block):

```csharp
public override void OnFrameworkInitializationCompleted()
{
    // ... setup code ...

#if DEBUG
    Svelonia.DevTools.SveloniaDevTools.Enable();
#endif

    base.OnFrameworkInitializationCompleted();
}
```

Press **F12** in your application window to open the DevTools overlay.
