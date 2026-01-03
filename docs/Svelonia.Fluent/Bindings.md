# Fluent Bindings & Events

## Data Binding

### One-Way Binding
```csharp
new TextBlock().BindText(node.DisplayText);
```

### Two-Way Binding
Commonly used with `TextBox`, `CheckBox`, and `Slider`.
```csharp
new TextBox().BindText(node.Text);
```

### Buffered Binding (Editing Pattern)
For scenarios where you want to edit a value but allow cancellation:
```csharp
var buffer = node.Text.ToBuffered();
new TextBox().BindBufferedText(buffer, node.IsEditing);
```
`BindBufferedText` automatically handles:
- **Enter**: Commits the buffer and sets `isEditing` to false.
- **Escape**: Resets the buffer and sets `isEditing` to false.

---

## Universal Event API

Avoid standard C# event syntax (`+=`) to keep your code fluent. Svelonia provides `.OnXXX` extensions for all routed events.

### Standard Events
```csharp
new Button()
    .OnClick(() => HandleClick())
    .OnPointerPressed(e => Log(e))
    .OnKeyDown(e => HandleKey(e));
```

### Accessing the Sender
If you need to access the control itself within the handler:
```csharp
new Grid()
    .OnPointerMoved((sender, e) => {
        var pos = e.GetPosition(sender);
    });
```

### Custom/Routed Events
You can attach any Avalonia `RoutedEvent`:
```csharp
control.On(Control.RequestBringIntoViewEvent, e => e.Handled = true);
```

---

## Lifecycle Events

```csharp
new MyComponent()
    .OnLoaded(e => InitializeData())
    .RegisterDisposable(someSubscription);
```
