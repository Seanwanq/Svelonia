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

## Reactive Effects

`Sve.Effect` allows you to run side effects whenever dependencies change. Unlike `Computed`, an effect does not return a value but is used for operations like logging, network requests, or manual UI manipulation.

```csharp
var count = new State<int>(0);

// Runs immediately and re-runs every time count.Value changes
using var cleanup = Sve.Effect(() => {
    Console.WriteLine($"The current count is: {count.Value}");
});

count.Value++; // Triggers the effect
cleanup.Dispose(); // Stops the effect and unsubscribes from all dependencies
```

### Dynamic Dependencies
Effects automatically track which states are read during execution. If your logic contains branches, Svelonia will only track the dependencies that were actually hit during the last run.

## Reactive Collections (`StateList<T>`)

`StateList<T>` is a reactive version of `ObservableCollection`. It integrates with Svelonia's dependency tracking and is the recommended source for `Sve.Each`.

```csharp
var list = new StateList<string>();
list.Add("New Item"); // Triggers dependent effects and UI lists
list.ReplaceAll(newItems); // Atomically updates the list with minimal UI churn
```

### Structural Tracking
`StateList<T>` includes a reactive `Version` property. When you iterate over a `StateList` or access its `Count` inside a `Computed` block, Svelonia automatically creates a dependency on the **collection's structure**.

```csharp
var items = new StateList<string>();
var countLabel = new Computed<string>(() => $"Total items: {items.Count}"); 
// Automatically recomputes when items are added or removed.
```

## Fine-Tuning Reactivity

### Skipping Tracking (`Sve.Untrack`)
Sometimes you want to read a state's value without creating a dependency. Use `Sve.Untrack` for this.

```csharp
Sve.Effect(() => {
    var current = count.Value; // This is tracked
    var snapshot = Sve.Untrack(() => otherState.Value); // This is NOT tracked
});
```

### Dynamic Resources (`Sve.Res`)
To use Avalonia DynamicResources fluently:

```csharp
new Border().Bg(Sve.Res("ThemeAccentBrush"))
```

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