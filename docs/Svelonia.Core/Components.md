[← Back to Svelonia.Core](./README.md)

# Components & Layout

## Component

`Svelonia.Core.Component` is the base class for all Svelonia UI components. It inherits from `Avalonia.Controls.UserControl` but adds simplified lifecycle and resource management.

### Resource Management (IDisposable)

In standard Avalonia, you often need to manually unsubscribe from events or dispose resources in `OnDetachedFromVisualTree`. `Component` simplifies this with the `Track` method.

```csharp
public class TimerComponent : Component
{
    private System.Timers.Timer _timer;

    public TimerComponent()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Start();

        // Automatically Dispose the timer when this component 
        // is removed from the screen (Detached).
        Track(_timer); 
    }
}
```

> **Debug Warning**: In DEBUG builds, Svelonia will automatically scan your component when it disposes. If it finds an `IDisposable` field (like `_timer` above) that you forgot to `Track()`, it will print a warning to the Debug Output.

### OnDispose

Override `OnDispose` to perform custom cleanup logic.

```csharp
protected override void OnDispose()
{
    Console.WriteLine("Component destroyed");
    base.OnDispose();
}
```

---

## Layout

`Layout` is a special type of component designed to wrap other content. It introduces the concept of a **Slot**.

### Defining a Layout

```csharp
using Svelonia.Core;

public class MainLayout : Layout
{
    public MainLayout(Control slot) : base(slot)
    {
        // 'Slot' is the content passed from the child page
        Content = new DockPanel()
            .SetChildren(
                new Sidebar().SetDock(Dock.Left),
                new Navbar().SetDock(Dock.Top),
                
                // Place the slot content
                Slot
            );
    }
}
```

### Using Layouts
(See [Routing Documentation](../Svelonia.Kit/Routing.md) for automatic layout application via folder structure).

Manual usage:
```csharp
var content = new HomePage();
var layout = new MainLayout(content);
```