[← Back to Svelonia.Fluent](./README.md)

# Basics

The core of Svelonia.Fluent is the automatic generation of extension methods for Avalonia controls.

## Fluent Syntax

Every public property of an Avalonia control is exposed as a method prefixed with `Set` that returns the control itself. This allows for method chaining and clearly distinguishes property setters from other operations.

```csharp
// Standard Avalonia (Object Initializer)
var btn = new Button
{
    Content = "Click Me",
    Width = 120,
    Height = 40
};

// Svelonia Fluent
var btn = new Button()
    .SetContent("Click Me")
    .SetWidth(120)
    .SetHeight(40);
```

## Tree Structure

Helpers like `SetChildren`, `SetChild`, and `SetContent` make building the visual tree intuitive.

### Panels (StackPanel, Grid, etc.)
Use `.SetChildren(...)` to add multiple child elements.

```csharp
new StackPanel()
    .SetSpacing(10)
    .SetChildren(
        new TextBlock().SetText("Header"),
        new Button().SetContent("Action")
    )
```

### Decorators (Border, ScrollViewer, etc.)
Use `.SetChild(...)` to set the single content element.

```csharp
new Border()
    .SetBorderThickness(1)
    .SetChild(
        new TextBlock().SetText("Inside Border")
    )
```

### ContentControls (Button, UserControl, etc.)
Use `.SetContent(...)` to set the content.

```csharp
new Button().SetContent("Text Content");

// Or complex content
new Button().SetContent(
    new StackPanel().SetChildren(
        new Icon(),
        new TextBlock().SetText("Icon Button")
    )
)
```

### Dynamic Resources (`Sve.Res`)
To use Avalonia DynamicResources fluently and safely (including in Native AOT environments):

```csharp
new Border().Bg(Sve.Res("ThemeAccentBrush"))
```
Svelonia's internal binding system automatically handles the conversion from resource extensions to live bindings, ensuring theme changes are reflected immediately.

## Advanced Setters

### Border Helpers
Quickly set both brush and thickness. Works on `Border` and `Button` (TemplatedControl).

```csharp
new Button()
    .SetBorder(Brushes.Red, thickness: 2.0);
```

### Canvas Positioning
Bind both `Canvas.Left` and `Canvas.Top` in one call.

```csharp
new Border()
    .BindPosition(node.X, node.Y); // node.X/Y are State<double>
```

## Event Handling

Common events have fluent wrappers.

### Universal OnClick

You can add a click handler to **any** control (Button, Border, Image, etc.).

```csharp
// Standard Button
new Button().Content("Save").OnClick(() => Console.WriteLine("Saved"));

// Interactive Card (Border)
new Border()
    .Background(Brushes.White)
    .OnClick(e => 
    {
        // 'e' is RoutedEventArgs
        Console.WriteLine("Card Clicked!");
    });
```
