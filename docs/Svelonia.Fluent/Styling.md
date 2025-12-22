[← Back to Svelonia.Fluent](./README.md)

# Styling

Svelonia.Fluent adopts a utility-first styling approach, offering helpers that make common layout and styling tasks faster.

## Colors (Tw)

The `Tw` static class provides a palette of colors inspired by Tailwind CSS.

```csharp
using Svelonia.Fluent;

new Border()
    .Background(Tw.Slate100)
    .BorderBrush(Tw.Blue500)
    .BorderThickness(2);
```

Available palettes include `Slate`, `Red`, `Blue`, `Green`, `Amber`, plus `White`, `Black`, and `Transparent`.

## Layout Helpers

### Spacing & Sizing
Simplified helpers for `Margin`, `Padding`, and `CornerRadius`.

```csharp
// Uniform
.Margin(10) 
.Padding(20)
.CornerRadius(8)

// Horizontal / Vertical
.Margin(10, 20) // 10px Horizontal, 20px Vertical

// Explicit (Left, Top, Right, Bottom)
.Margin(5, 10, 5, 0)
```

### Grid System
Fluent helpers for defining rows and columns and assigning positions.

**Defining the Grid:**
```csharp
new Grid()
    .Cols("Auto, *, 2*") // ColumnDefinitions
    .Rows("Auto, Auto")  // RowDefinitions
```

**Positioning Elements (Attached Properties):**
```csharp
new TextBlock()
    .Col(0)      // Grid.Column="0"
    .Row(1)      // Grid.Row="1"
    .ColSpan(2)  // Grid.ColumnSpan="2"
```

### DockPanel
```csharp
new Button().Dock(Dock.Left)
```

## State-Based Styling (Hover, Pressed, Focus)
Many property extensions support `hover`, `pressed`, and `focus` arguments for simple state-based styling without creating complex XAML templates.

```csharp
new Button()
    .Background(Tw.Blue500, hover: Tw.Blue600, pressed: Tw.Blue700)
    .Foreground(Tw.White, disabled: Tw.Gray400)
```

### Borders
You can style borders concisely using the `.Border()` shorthand or individual extensions.

```csharp
new TextBox()
    // Shorthand: Brush, Thickness, FocusBrush
    .Border(Tw.Slate300, 1, focusBrush: Tw.Blue500)
    .CornerRadius(4);
```

### Deep Styling for Complex Controls
Svelonia.Fluent automatically "drills down" into the template of certain complex controls to apply styles correctly.

*   **TextBox**: Styles like `BorderBrush` or `Background` applied with state (e.g., `focus`) will target the internal `PART_Border`. This ensures your focus styles work as expected even if the default theme uses TemplateBindings.
*   **CheckBox**: Styles target the internal `NormalRectangle` (the actual box), allowing you to change the check box color without affecting the text background.

```csharp
// The background color changes only for the box itself on hover
new CheckBox()
    .Background(Tw.White, hover: Tw.Slate100)
```