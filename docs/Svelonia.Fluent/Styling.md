[← Back to Svelonia.Fluent](./README.md)

# Styling

Svelonia provides a fluent API for styling controls, including handling visual states (Hover, Focus, Pressed) and theme resources.

## Atomic & Basic Properties

Most styling properties are available as extension methods. Svelonia provides highly optimized "Atomic" shorthands for the most common properties, alongside generated `SetX` methods for everything else.

```csharp
new Border()
    .Bg("#ff0000")          // Shorthand for Background (Atomic)
    .SetCornerRadius(8)     // Generated Setter
    .SetPadding(10.5);      // Generated Setter
```

### 💡 Atomic Helpers (Shorthands)
For the most frequently used styles, use these short, optimized helpers:
- `.Bg(brush, ...)`: Background
- `.Fg(brush, ...)`: Foreground
- `.Rounded(units)`: CornerRadius
- `.P(uniform)` / `.P(h, v)`: Padding

## State-Based Styling

You can define styles for different states inline. These also support `State<T>` and `Computed<T>` for reactive styling.

```csharp
var isSelected = new State<bool>(false);

new Button()
    .Bg(
        normal: isSelected.Select(s => s ? Brushes.Gold : Brushes.Gray),
        hover: Brushes.LightBlue
    );
```

Supported states in `Bg`, `Fg`, and other style helpers: `normal`, `hover`, `pressed`, `disabled`, `focus`.

## Skeuomorphic Effects (BoxShadow)

`SetBoxShadow` is supported on `Border` controls and can be themed.

```csharp
new Border()
    .SetBoxShadow(Sve.Res("ButtonShadow"))
    .SetCornerRadius(10)
    .SetChild(...);
```

## Lightweight Styling (Theme Resources)

For complex controls like `TextBox` or `Button` in the Fluent Theme, simple property overrides (like `SetBorderThickness`) might be ignored because the internal template binds to specific Theme Resources. Svelonia automatically maps these properties to the correct internal resource keys.

```csharp
// This automatically sets internal resource overrides for the TextBox
new TextBox()
    .SetBorderThickness(0, hover: 0, pressed: 0)
    .Bg(Brushes.Transparent);
```

### 💡 Implicit Conversion

Thanks to `SveConverter`, you no longer need to manually wrap values like `new Thickness()` or `new CornerRadius()`. The following is now safe and recommended:

```csharp
// ✅ Safe - Auto-converts to Thickness
.SetBorderThickness(0) 

// ✅ Safe - Auto-converts to CornerRadius
.SetCornerRadius(8)
```

## Semantic Design System (G Class)

Svelonia introduces a global `G` class to manage semantic design constants (Spacing, Radius, Sizes). This replaces magic numbers with meaningful names and allows for global design adjustments.

```csharp
using Svelonia.Fluent;

// Use semantic presets
new StackPanel()
    .SetSpacing(G.Medium)    // 16.0
    .SetMargin(G.Large);     // 24.0

new Border()
    .Rounded(G.RadiusSmall)  // Atomic helper for CornerRadius
    .SetPadding(G.Small);    // Generated Setter
```

### Configuration

You can override these values at application startup to match your design system:

```csharp
// App.cs or Program.cs
G.Small = 10.0;
G.Medium = 20.0;
G.RadiusSmall = 6.0;
```

## Drill-Down Styling (Advanced)

For properties that don't map to resources, Svelonia attempts to "drill down" into the template (e.g., finding `PART_Border`). This is handled automatically by the fluent helpers.

## Atomic CSS-like Classes

You can also use atomic classes if you have configured `AtomicTheme`.

```csharp
new Button().Classes("bg-blue-500 text-white p-4 rounded");
```