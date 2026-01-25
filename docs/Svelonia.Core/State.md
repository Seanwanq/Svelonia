# Svelonia State Management

Svelonia provides a fine-grained reactivity system inspired by Svelte and SolidJS.

## Basic State

`State<T>` is the fundamental building block. It wraps a value and notifies subscribers when it changes.

```csharp
var count = new State<int>(0);
count.Value += 1; // Notifies all UI bindings and effects
```

### Silent Updates
If you need to update a state without triggering global debug notifications (useful for internal framework logic or preventing infinite loops in DevTools), use `SetSilent`.

```csharp
state.SetSilent(newValue); // Updates value and notifies observers, but skips StateDebug
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

### Paused Effects (Manual Activation)
By default, an `Effect` runs immediately upon creation. However, in scenarios like **bulk initialization** or **object pooling**, you might want to create an effect but delay its execution.

```csharp
// Create an effect but don't run it yet
var effect = Sve.Effect(() => {
    Console.WriteLine("Layout calculation...");
}, paused: true);

// ... perform bulk operations ...

// Manually activate the effect
// This runs the action for the first time and registers dependencies
effect.Resume();
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

### Hierarchy Management (`HierarchyStateList<T>`)

When building tree structures (like mind maps or org charts), maintaining parent-child relationships manually is error-prone. Svelonia provides a specialized collection to handle this.

1.  **Implement `IHierarchyNode`**:
    ```csharp
    public class MyNode : IHierarchyNode {
        public State<MyNode?> Parent { get; } = new(null);
        IState IHierarchyNode.ParentState => Parent;
    }
    ```

2.  **Use `HierarchyStateList`**:
    ```csharp
    public class MyNode {
        public HierarchyStateList<MyNode> Children { get; }
        public MyNode() { Children = new HierarchyStateList<MyNode>(this); }
    }
    ```

Whenever a node is added to the `Children` list, the collection automatically updates the child's `ParentState` to point to the owner. It also handles clearing the parent when items are removed.

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