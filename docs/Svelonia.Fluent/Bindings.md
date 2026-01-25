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

### Polymorphic MapToChildren
The `MapToChildren` extension is the core of Svelonia list rendering. It supports two types of sources:

1.  **StateList<T>**: High-performance incremental updates based on collection events.
2.  **State<IEnumerable<T>>** (e.g. `Computed`): Automatically performs a Diff between the old and new list to synchronize the Visual Tree efficiently.

```csharp
// Using a filtered Computed list
var searchResults = new Computed<IEnumerable<Item>>(() => 
    allItems.Where(x => x.Name.Contains(query.Value))
);

new StackPanel().MapToChildren(searchResults, item => new ItemView(item));
```

### Universal Property Binding
Svelonia uses a high-performance Source Generator (`Svelonia.Gen`) to automatically create fluent extensions for every Avalonia property.

1.  **Generated Extensions**: Use the standard `BindXXX` or `SetXXX` names.
    ```csharp
    new Border().BindWidth(node.Width); // Generated automatically
    ```

2.  **Manual/Framework Extensions**: To avoid naming collisions with generated code, Svelonia's manual library extensions use the `Sve` prefix.
    ```csharp
    new Border().SveBindIsVisible(node.IsVisible); // Manual framework version
    ```

> **Rule of Thumb**: Always try the standard `BindXXX` first. If you encounter an "Ambiguous call" or "Definition not found" error, use the `SveBindXXX` equivalent.

For any property not covered by either, use the generic `BindState`:
```csharp
control.SveBindState(Layoutable.MarginProperty, someState);
```

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
