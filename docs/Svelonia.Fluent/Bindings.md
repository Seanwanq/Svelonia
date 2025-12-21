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
Use `.BindTextContent(...)` or simply pass a `State<string>` to `.Text(...)`.

```csharp
State<string> message = new("Hello");

// Option A: Generated Extension
new TextBlock().Text(message);

// Option B: Explicit Helper (useful for Computed conversions)
new TextBlock().BindTextContent(message);
```

### TextBox (Two-Way)
For input controls, use `.BindText(...)` to enable two-way data binding.

```csharp
State<string> username = new("");

new TextBox()
    .BindText(username); // Typing updates 'username.Value'
```

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
