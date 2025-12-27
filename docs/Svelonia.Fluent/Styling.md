[← Back to Svelonia.Fluent](./README.md)

# Styling

Svelonia provides a fluent API for styling controls, including handling visual states (Hover, Focus, Pressed) and theme resources.

## Basic Properties

Most styling properties are available as extension methods. Svelonia includes an intelligent conversion layer (`SveConverter`) that simplifies passing values.

```csharp
new Border()
    .Background("#ff0000") // Auto-converts string to SolidColorBrush
    .CornerRadius(8)       // Auto-converts int to CornerRadius
    .Padding(10.5);        // Auto-converts double to Thickness
```

## State-Based Styling

You can define styles for different states inline. These also support `State<T>` and `Computed<T>` for reactive styling.

```csharp
var isSelected = new State<bool>(false);

new Button()
    .Background(
        normal: isSelected.Select(s => s ? Brushes.Gold : Brushes.Gray),
        hover: Brushes.LightBlue
    );
```

Supported states: `normal`, `hover`, `pressed`, `disabled`, `focus`.

## Skeuomorphic Effects (BoxShadow)

`BoxShadow` is supported on `Border` controls and can be themed.

```csharp
new Border()
    .BoxShadow(Sve.Res("ButtonShadow"))
    .CornerRadius(10)
    .Child(...);
```

## Lightweight Styling (Theme Resources)

For complex controls like `TextBox` or `Button` in the Fluent Theme, simple property overrides (like `BorderThickness`) might be ignored because the internal template binds to specific Theme Resources. Svelonia automatically maps these properties to the correct internal resource keys.

```csharp
// This automatically sets internal resource overrides for the TextBox
new TextBox()
    .BorderThickness(0, focus: 0)
    .Background(Brushes.Transparent);
```

### 💡 Implicit Conversion

Thanks to `SveConverter`, you no longer need to manually wrap values like `new Thickness()` or `new CornerRadius()`. The following is now safe and recommended:

```csharp
// ✅ Safe - Auto-converts to Thickness
.BorderThickness(0) 

// ✅ Safe - Auto-converts to CornerRadius
.CornerRadius(8)
```

## Drill-Down Styling (Advanced)

For properties that don't map to resources, Svelonia attempts to "drill down" into the template (e.g., finding `PART_Border`). This is handled automatically by the fluent helpers.

## Atomic CSS-like Classes

You can also use atomic classes if you have configured `AtomicTheme`.

```csharp
new Button().Classes("bg-blue-500 text-white p-4 rounded");
```
