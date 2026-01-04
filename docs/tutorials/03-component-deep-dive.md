# Custom Component Deep Dive

Building a reusable component in Svelonia requires blending Avalonia's powerful control system with Svelonia's fine-grained reactivity. This tutorial uses the `InfiniteCanvas` as a case study.

## 1. Choosing the Right Base Class

*   **Control**: For primitive elements (like a custom-drawn shape).
*   **ContentControl**: For wrappers that host a single child (like `InfiniteCanvas` or a `Card`).
*   **Panel**: For containers that manage multiple children (like a custom `Grid`).

For `InfiniteCanvas`, we chose `ContentControl` because it provides a `Content` property out of the box, which we can then transform.

## 2. Exposing Reactive State

Don't use standard Avalonia DependencyProperties if you want Svelonia-style reactivity. Instead, expose `State<T>` properties.

```csharp
public class MyComponent : ContentControl 
{
    // Users can bind to this or read it directly
    public State<double> MyProperty { get; } = new(1.0);
}
```

## 3. Intercepting Property Changes

In Avalonia, you often need to react when a property (like `Content`) is assigned. Override `OnPropertyChanged` to handle this.

```csharp
protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
{
    base.OnPropertyChanged(change);

    if (change.Property == ContentProperty)
    {
        if (change.NewValue is Control control)
        {
            // Set up bindings on the child here
            // Example: Link child transform to parent state
            control.Bind(Visual.RenderTransformProperty, 
                MyProperty.Select(v => new ScaleTransform(v, v)).ToBinding());
        }
    }
}
```

## 4. Advanced Interaction

Use Svelonia's Fluent Event extensions even inside your component for cleaner code, but remember to handle `Focus` and `Pointer Capture`.

### Panning Logic Example:
1.  **OnPressed**: Check for the right button, capture the pointer (`e.Pointer.Capture(this)`), and record the starting position.
2.  **OnMoved**: If captured, calculate the delta and update your internal `Matrix` state.
3.  **OnReleased**: Release the capture.

## 5. Lifecycle & Cleanup

If your component subscribes to external events or timers, use the `RegisterDisposable` extension (available on all Controls in Svelonia) to ensure they are cleaned up when the component is removed from the screen.

```csharp
public MyComponent()
{
    var timer = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(...);
    this.RegisterDisposable(timer); // Automatic cleanup
}
```

## 6. Making it Fluent-Friendly

To make your component feel "native" to Svelonia, ensure your state properties are easy to chain. You can also add custom extension methods in a separate static class to provide helpers like `.BindMyProperty(state)`.

---

### Summary: The Component Checklist
1.  Is the **Background** set to `Transparent`? (Otherwise, it won't receive mouse events in empty areas).
2.  Is **ClipToBounds** enabled if you are doing transforms?
3.  Are you using **MatrixTransform** for smooth movement?
4.  Are all external resources registered for **Disposal**?
