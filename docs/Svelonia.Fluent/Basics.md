[← Back to Svelonia.Fluent](./README.md)

# Basics

The core of Svelonia.Fluent is the automatic generation of extension methods for Avalonia controls.

## Fluent Syntax

Every public property of an Avalonia control is exposed as a method that returns the control itself. This allows for method chaining.

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
    .Content("Click Me")
    .Width(120)
    .Height(40);
```

## Tree Structure

Helpers like `Children`, `Child`, and `Content` make building the visual tree intuitive.

### Panels (StackPanel, Grid, etc.)
Use `.Children(...)` to add multiple child elements.

```csharp
new StackPanel()
    .Spacing(10)
    .Children(
        new TextBlock().Text("Header"),
        new Button().Content("Action")
    )
```

### Decorators (Border, ScrollViewer, etc.)
Use `.Child(...)` to set the single content element.

```csharp
new Border()
    .BorderThickness(1)
    .Child(
        new TextBlock().Text("Inside Border")
    )
```

### ContentControls (Button, UserControl, etc.)
Use `.Content(...)` to set the content.

```csharp
new Button().Content("Text Content");

// Or complex content
new Button().Content(
    new StackPanel().Children(
        new Icon(),
        new TextBlock().Text("Icon Button")
    )
)
```

## Event Handling

Common events have fluent wrappers, most notably `.OnClick(...)` for buttons.

```csharp
new Button()
    .Content("Save")
    .OnClick(e => 
    {
        Console.WriteLine("Button Clicked!");
    })
```
