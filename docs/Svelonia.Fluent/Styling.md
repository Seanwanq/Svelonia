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

## Hover & Pressed States
Many property extensions support `hover` and `pressed` arguments for simple state-based styling without complex templates.

```csharp
new Button()
    .Background(Tw.Blue500, hover: Tw.Blue600, pressed: Tw.Blue700)
```
*(Note: Support for this feature depends on the specific control and generated extension availability)*
