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
    .BindFontSize(fontSize)
    .SetText("Responsive Text");
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
        return new Grid().SetCols("250, *").SetChildren(
            new Border().SetCol(0).SetChild(new TextBlock().SetText("Sidebar")),
            new Border().SetCol(1).SetChild(Slot) // Display content
        );
    }
    else
    {
        // Mobile: Content + Bottom Nav
        return new Grid().SetRows("*, Auto").SetChildren(
            new Border().SetRow(0).SetChild(Slot),
            new Border().SetRow(1).SetChild(new TextBlock().SetText("Bottom Nav"))
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

new StackPanel().BindOrientation(layout);
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

new TextBlock().BindFontFamily(fontFamily);
```

You can also use boolean flags: `Platform.IsWindows`, `Platform.IsAndroid`, etc.
