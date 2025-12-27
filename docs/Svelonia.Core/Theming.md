[← Back to Svelonia.Core](./README.md)

# Theming System

Svelonia includes a lightweight, JSON-based theming engine (`SveloniaTheme`) designed for easy runtime switching and hot-swapping.

## Features

- **JSON Format**: Define your design tokens (colors, spacing, radius) in standard JSON.
- **Auto-Conversion**: Automatically converts hex strings to `SolidColorBrush`, and numbers to `Thickness` or `CornerRadius` based on key names.
- **Hot Swapping**: Switch between Light/Dark themes instantly using `State<T>`.

## Defining a Theme

Create a JSON file (e.g., `light.json`):

```json
{
  "PrimaryColor": "#3b82f6",
  "BackgroundColor": "#ffffff",
  "TextColor": "#1f2937",
  
  "SmallRadius": 4, 
  "MediumPadding": 16,
  "BorderWidth": 1
}
```

*   **Colors**: Strings starting with `#` are parsed as colors.
*   **Thickness/Radius**: Numbers assigned to keys containing "Radius", "Padding", "Margin", or "Thickness" are automatically converted to their respective Avalonia types.

## Setup in App.cs

Use `SveloniaTheme.Setup` to wire up automatic theme switching based on the system theme or user preference.

```csharp
public class App : Application
{
    // Define paths or filenames for your themes
    public static readonly State<string> LightTheme = new("Themes/light.json");
    public static readonly State<string> DarkTheme = new("Themes/dark.json");

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        
        // Initialize the theme manager
        SveloniaTheme.Setup(this, LightTheme, DarkTheme);
    }
}
```

## Manual Loading

You can also load themes manually from a string or file.

```csharp
// Load from a JSON string
SveloniaTheme.Load("{\"MyColor\": \"#ff0000\"}");

// Load from a file path
SveloniaTheme.LoadFromFile("path/to/theme.json");

// Apply specific theme file immediately
SveloniaTheme.Apply("Themes/custom.json", ThemeVariant.Dark);
```

## Type-Safe Access

While resources are typically used via bindings, you can access them safely in code using `Get<T>`:

```csharp
// Throws exception if key is missing or type is wrong
var color = SveloniaTheme.Get<SolidColorBrush>("PrimaryColor");
var margin = SveloniaTheme.Get<Thickness>("MediumPadding");
```
