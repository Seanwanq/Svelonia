[← Back to Svelonia.Core](./README.md)

# Components & Layout

## Component

`Svelonia.Core.Component` is the base class for all Svelonia UI components. It inherits from `Avalonia.Controls.UserControl` but adds simplified lifecycle and resource management.

### Resource Management (IDisposable)

In standard Avalonia, you often need to manually unsubscribe from events or dispose resources in `OnDetachedFromVisualTree`. `Component` simplifies this with the `Track` method.

```csharp
public class TimerComponent : Component
{
    public TimerComponent()
    {
        var timer = new System.Timers.Timer(1000);
        timer.Start();

        // Automatically Dispose the timer when this component 
        // is removed from the screen (Detached).
        Track(timer); 
    }
}
```

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
            .Children(
                new Sidebar().Dock(Dock.Left),
                new Navbar().Dock(Dock.Top),
                
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
