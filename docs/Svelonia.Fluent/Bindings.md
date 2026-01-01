[← Back to Svelonia.Fluent](./README.md)

# Bindings

Svelonia.Fluent integrates tightly with `Svelonia.Core` to provide seamless data binding.

## Explicit Binding

Most generated fluent methods have a corresponding `BindX` method that accepts `State<T>`. When used, the property is automatically bound to the state.

```csharp
State<double> width = new(100);

new Rectangle()
    .BindWidth(width)      // Binds Width property to state
    .BindHeight(width);    // Binds Height property to state
```

## Text Binding

### TextBlock (One-Way)

```csharp
// Bind text content to a string state or computed value
new TextBlock().BindText(myState);
```

## TextBox (Two-Way)

For input controls like `TextBox`, use `.BindText` to enable two-way binding. Changes in the UI update the State, and changes in the State update the UI.

```csharp
var name = new State<string>("Alice");

new TextBox()
    .BindText(name); // Two-way
```

## Content Binding

You can bind the content of a Panel or ContentControl to a state using the `Bind` prefix.

```csharp
State<Control> currentView = new(new LoginView());

new ContentControl().BindContent(currentView);
```

Or for lists of children in a Panel:

```csharp
State<IEnumerable<Control>> items = new(...);

new StackPanel().BindChildren(items);
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

### Focusing
To ensure a control (like a Panel or Border) can receive keyboard events, use `.SetFocusable()`.

```csharp
new StackPanel()
    .SetFocusable() // Enables focus
    .OnKey("A", () => Console.WriteLine("A pressed"))
    .SetChildren(...);
```