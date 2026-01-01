[← Back to Svelonia.Core](./README.md)

# State Management

Svelonia uses a **fine-grained reactivity** model. Unlike `INotifyPropertyChanged` which signals that "something changed", Svelonia's `State<T>` and `Computed<T>` create a precise dependency graph.

## State\<T>

`State<T>` is the primitive for holding reactive data.

### Usage

```csharp
using Svelonia.Core;

// 1. Definition
// Create a state with an initial value
var count = new State<int>(0);

// 2. Reading
// Accessing .Value automatically registers dependencies if called within a Computed context.
Console.WriteLine(count.Value);

// 3. Writing
// Modifying .Value triggers notifications to all listeners.
count.Value++; 
```

### Binding to UI
(See `Svelonia.Fluent` documentation for `Bind` syntax details)
```csharp
new TextBlock().BindText(count); // Updates automatically
```

---

## Computed\<T>

`Computed<T>` is a read-only state that derives its value from other states. It **automatically tracks dependencies**.

### Usage

```csharp
var firstName = new State<string>("John");
var lastName = new State<string>("Doe");

// fullName will automatically re-evaluate whenever firstName OR lastName changes.
var fullName = new Computed<string>(() => $"{firstName.Value} {lastName.Value}");

Console.WriteLine(fullName.Value); // "John Doe"

firstName.Value = "Jane";
Console.WriteLine(fullName.Value); // "Jane Doe"
```

### Dynamic Dependencies
`Computed` only listens to dependencies accessed during the *last* evaluation.

```csharp
var showDetails = new State<bool>(false);
var details = new State<string>("Secret");

var view = new Computed<string>(() => {
    // If showDetails is false, 'details' is never accessed, 
    // so changes to 'details' will NOT trigger a re-computation.
    return showDetails.Value ? details.Value : "Hidden";
});
```

---

## StateList\<T>

A reactive collection wrapper around `ObservableCollection<T>`. It ensures collection change notifications are marshaled to the UI thread.

### Usage

```csharp
var items = new StateList<string>();

items.Add("Item 1"); // Safe to call from background threads
```

### Batch Updates
Use `AddRange` to add multiple items while triggering only one UI refresh notification.

```csharp
items.AddRange(new[] { "A", "B", "C" });
```

---

## Extensions

### Select
Create a lightweight computed property from a single state.

```csharp
State<int> count = new(10);
Computed<string> label = count.Select(c => $"Count is {c}");
```

### Match
Functional pattern matching for state values.

```csharp
State<Status> status = new(Status.Loading);

Computed<string> statusText = status.Match(
    (Status.Loading, "Please wait..."),
    (Status.Success, "Done!"),
    (Status.Error, "Something went wrong")
);

// With default fallback
Computed<string> safeText = status.Match("Unknown",
    (Status.Success, "Done!")
);
```
