[← Back to Svelonia.Fluent](./README.md)

# Bindings

Svelonia.Fluent integrates tightly with `Svelonia.Core` to provide seamless data binding.

## Implicit Binding

Most generated fluent methods have an overload that accepts `State<T>`. When used, the property is automatically bound to the state.

```csharp
State<double> width = new(100);

new Rectangle()
    .Width(width)      // Binds Width property to state
    .Height(width);    // Binds Height property to state
```

## Text Binding

### TextBlock (One-Way)

```csharp
new TextBlock().BindTextContent(myState);
```

## TextBox (Two-Way)

For input controls like `TextBox`, use `.BindText` to enable two-way binding. Changes in the UI update the State, and changes in the State update the UI.

```csharp
var name = new State<string>("Alice");

new TextBox()
    .BindText(name); // Two-way
```

## Styling Bindings

## Content Binding

You can bind the content of a Panel or ContentControl to a state.

```csharp
State<Control> currentView = new(new LoginView());

new ContentControl().Content(currentView);
```

Or for lists of children:

```csharp
State<IEnumerable<Control>> items = new(...);

new StackPanel().Children(items);
```

## Keyboard Events

You can handle keyboard events fluently using the `.OnKey(...)` extension.

### Basic Key Binding
Support for string parsing (KeyGesture) and simple key names.

```csharp
new TextBox()
    .OnKey("Enter", SubmitForm) // Matches Enter key
    .OnKey("Ctrl+S", Save);     // Matches Ctrl+S gesture
```

### Strong-Typed Binding
You can also use the `Key` enum directly.

```csharp
new Button()
    .OnKey(Key.Escape, CloseDialog);
```

### Focusing
To ensure a control (like a Panel or Border) can receive keyboard events, use `.Focusable()`.

```csharp
new StackPanel()
    .Focusable() // Enables focus
    .OnKey("A", () => Console.WriteLine("A pressed"))
    .Children(...);
```