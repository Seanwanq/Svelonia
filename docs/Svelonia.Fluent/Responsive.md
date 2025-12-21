[← Back to Svelonia.Fluent](./README.md)

# Responsive Design

Building applications that adapt to different screen sizes (Mobile, Tablet, Desktop) or platforms (Windows, Android) is built into Svelonia.Fluent.

## Setup

To ensure `MediaQuery` receives window resize events, you must hook up the `Update` method in your `App.cs`.

```csharp
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        // ... create MainWindow ...
        
        // Register Resize Listener
        desktop.MainWindow.SizeChanged += (s, e) => MediaQuery.Update(e.NewSize);
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
    {
        // For Mobile/Web
        if (singleView.MainView != null)
        {
             singleView.MainView.SizeChanged += (s, e) => MediaQuery.Update(e.NewSize);
        }
    }
    
    base.OnFrameworkInitializationCompleted();
}
```

## MediaQuery

The `MediaQuery` class provides reactive states for the window dimensions.

### Screen Types
Available breakpoinst: `Mobile`, `Tablet`, `Desktop`, `Large`, `ExtraLarge`.

```csharp
// Returns a Computed<T> that changes based on screen width
var fontSize = MediaQuery.Select(
    mobile: 14,
    desktop: 18
);

new TextBlock()
    .FontSize(fontSize)
    .Text("Responsive Text");
```

### Boolean Flags
You can use boolean flags in `IfControl` or other logic.

```csharp
new IfControl(MediaQuery.IsMobile, 
    () => new MobileMenu(),
    () => new DesktopSidebar()
);
```

### Dynamic Layouts

For complex scenarios where the entire layout structure needs to change (e.g., Sidebar on Desktop vs. Bottom Nav on Mobile), you can bind `Content` directly to a `Computed<Control>`.

```csharp
// In a Layout or Page constructor
Content = new Computed<Control>(() =>
{
    // Reactively rebuilds when Width changes
    if (MediaQuery.Width.Value > 600)
    {
        // Desktop: Sidebar + Content
        return new Grid().Cols("250, *").Children(
            new Border().Col(0).Child(new TextBlock().Text("Sidebar")),
            new Border().Col(1).Child(Slot) // Display content
        );
    }
    else
    {
        // Mobile: Content + Bottom Nav
        return new Grid().Rows("*, Auto").Children(
            new Border().Row(0).Child(Slot),
            new Border().Row(1).Child(new TextBlock().Text("Bottom Nav"))
        );
    }
});
```

### Orientation
Detect Portrait vs Landscape.

```csharp
var layout = MediaQuery.OnOrientation<StackOrientation>(
    portrait: StackOrientation.Vertical,
    landscape: StackOrientation.Horizontal
);

new StackPanel().Orientation(layout);
```

## Platform Detection

The `Platform` helper allows you to tailor UI for specific operating systems.

```csharp
var fontFamily = Platform.Select(
    windows: "Segoe UI",
    macos: "San Francisco",
    android: "Roboto",
    @default: "Arial"
);

new TextBlock().FontFamily(fontFamily);
```

You can also use boolean flags: `Platform.IsWindows`, `Platform.IsAndroid`, etc.
