[← Back to Kit Index](./README.md)

# Svelonia Theming System

Svelonia provides a robust, JSON-based theming system that separates design tokens from code while maintaining type safety through helper classes.

## 1. JSON Theme Definition

Themes are defined in JSON files located in strict directory structures (e.g., `Assets/Themes/default.json`).

```json
{
  "Theme.Primary": "#007AFF",
  "Theme.Background": "#F5F5F7",
  "Theme.Text": "#1D1D1F",
  "Theme.CornerRadius": 12.0
}
```

- Keys must start with a prefix (e.g., `Theme.`) for organization.
- Values can be Hex Colors (`#RRGGBB` or `#AARRGGBB`) or numeric values (doubles).

## 2. Setup

Initialize the theme system in your `App.cs` during `OnFrameworkInitializationCompleted`:

```csharp
// Define a State for runtime theme switching
public static State<string> CurrentTheme = new("default");

public override void OnFrameworkInitializationCompleted()
{
    // ...
    // SveloniaTheme.Setup(Application app, State<string> themeName, State<string> variant, string basePath)
    SveloniaTheme.Setup(this, CurrentTheme, CurrentTheme, "Assets/Themes");
    // ...
}
```

## 3. Type-Safe Resources (The `R` Class)

Instead of using "Magic Strings" in your code (e.g., `"Theme.Primary"`), create a static `R` (Resources) class to expose them as `DynamicResourceExtension`.

```csharp
using Avalonia.Markup.Xaml.MarkupExtensions;
using Svelonia.Fluent; // For Tw helper

public static class R
{
    // Colors
    public static DynamicResourceExtension Primary => Tw.Resource("Theme.Primary");
    public static DynamicResourceExtension Background => Tw.Resource("Theme.Background");
    
    // Metrics
    public static DynamicResourceExtension CornerRadius => Tw.Resource("Theme.CornerRadius");
}
```

**Usage:**

```csharp
new Button().Bg(R.Primary).SetCornerRadius(R.CornerRadius);
```

## 4. Semantic Constants (The `G` Class)

For layout consistency (margins, padding, sizing), defining a `G` (Global/Geometry) class is recommended. These are usually compile-time constants, not dynamic resources.

```csharp
public static class G
{
    // Spacing / Padding
    public const double Small = 4;
    public const double Medium = 8;
    public const double Large = 16;
    public const double XLarge = 24;

    // Radius
    public const double RadiusSmall = 4;
    public const double RadiusMedium = 8;
    public const double RadiusLarge = 12;
}
```

**Usage:**

```csharp
new StackPanel()
    .SetSpacing(G.Medium)
    .P(G.Large);
```

## Runtime Switching

Simply update the state passed to `Setup`:

```csharp
App.CurrentTheme.Value = "dark"; // Loads assets/Themes/dark.json
```

The system will automatically reload the JSON resources and update all `DynamicResource` bindings in the UI.
