# Svelonia State Management

Svelonia provides a fine-grained reactivity system inspired by Svelte and SolidJS.

## Basic State

`State<T>` is the fundamental building block. It wraps a value and notifies subscribers when it changes.

```csharp
var count = new State<int>(0);
count.Value += 1; // Notifies all UI bindings and effects
```

## Computed State

`Computed<T>` derives a value from other states. It automatically tracks dependencies and recomputes only when necessary.

```csharp
var firstName = new State<string>("John");
var lastName = new State<string>("Doe");
var fullName = new Computed<string>(() => $"{firstName.Value} {lastName.Value}");
```

> **Note**: Svelonia's `Computed` is robust against high-frequency synchronous updates and handles recursive dependencies gracefully using a "dirty bit" mechanism.

## Buffered State

`BufferedState<T>` is a special wrapper used for "draft" scenarios (like edit forms). It holds a temporary value that can be either committed back to the source or reset.

```csharp
var originalText = new State<string>("Hello");
var buffer = originalText.ToBuffered();

buffer.Value = "Changes..."; // Original is NOT updated yet
buffer.Commit(); // Syncs back to original
// OR
buffer.Reset();  // Reverts buffer to original's value
```

## Advanced Features

### INotifyPropertyChanged Integration
All `State<T>` objects implement `INotifyPropertyChanged`. This means Svelonia states are fully compatible with native Avalonia bindings and controls.

### Force Tracking
Internal logic inside `Computed` and `Effect` is wrapped in `ForceTrack`, ensuring that reactive dependencies are always established even if initialized within a `Sve.Untrack` block.