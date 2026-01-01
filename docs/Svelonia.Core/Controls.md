[← Back to Svelonia.Core](./README.md)

# Functional Controls

Svelonia encourages "Logic as UI". Instead of writing complex C# logic in `Render` methods or Code-behind to manipulate the visual tree, you can use functional controls.

## IfControl

Conditionally renders a control based on a boolean `State`.

### Basic Usage

```csharp
var isLoggedIn = new State<bool>(false);

var view = new IfControl(
    condition: isLoggedIn,
    builder: () => new DashboardView()
);
```
*   **condition**: The boolean state to watch.
*   **builder**: A factory function that creates the control when the condition becomes `true`. The control is disposed when the condition becomes `false`.

### With Animations

`IfControl` supports enter/exit transitions.

```csharp
using Svelonia.Core.Animation;

new IfControl(
    isLoggedIn,
    () => new DashboardView(),
    transition: Transition.Fade(duration: 500)
);
```

---

## AwaitControl\<T>

Simplifies handling asynchronous tasks in the UI, managing Loading, Success, and Error states automatically.

### Usage

```csharp
Task<UserProfile> LoadProfile() => apiClient.GetUserAsync();

// Use a factory lambda to allow reloading
var loader = new AwaitControl<UserProfile>(
    taskFactory: () => LoadProfile(),
    loading: () => new TextBlock().SetText("Loading..."),
    then: (user) => new ProfileCard(user),
    error: (ex) => new TextBlock().SetText($"Error: {ex.Message}")
);

// Trigger a refresh later
loader.Reload();
```

*   **taskFactory**: A function that returns a `Task<T>`. This is called immediately and whenever `Reload()` is called.
*   **loading**: (Optional) View to show while the task is running.
*   **then**: Function to build the Success view using the result.
*   **error**: (Optional) Function to build the Error view.
