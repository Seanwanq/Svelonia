# Fluent Styling

Svelonia uses a chainable API to describe styles and property setters.

## Basic Properties

```csharp
new Border()
    .Bg(Brushes.White)
    .Fg(Brushes.Black)
    .Opacity(0.8)
    .CornerRadius(6);
```

## State-based Styling

You can bind styles directly to reactive states:

```csharp
var isDark = new State<bool>(false);
var bg = new Computed<IBrush>(() => isDark.Value ? Brushes.Black : Brushes.White);

new Grid().Bg(bg);
```

## State Shorthands (WhenXXX)

To distinguish between **Logic Events** and **Visual Styling**, Svelonia uses the `When` prefix for pseudo-class styles.

### Hover
```csharp
new Button()
    .WhenHovered(style => style.Bg(Brushes.Red));
```

### Pressed (Universal)
Native Avalonia only supports `:pressed` on specific controls like Buttons. Svelonia provides **Universal Pressed Support** for all controls (Border, Grid, etc.) by automatically managing a `.pressed` class.

```csharp
new Border()
    .WhenPressed(style => style.Scale(0.95)); // Works on ANY control
```

### Disabled
```csharp
new TextBox()
    .WhenDisabled(style => style.Opacity(0.5));
```

## Atomic Styling (Tailwind-like)

Svelonia provides atomic shortcuts for rapid UI development:

```csharp
new StackPanel()
    .P(2)      // Padding
    .M(1)      // Margin
    .Rounded() // CornerRadius
    .Bg(Brushes.Blue, hover: Brushes.LightBlue, pressed: Brushes.DarkBlue);
```
