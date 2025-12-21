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

var view = new AwaitControl<UserProfile>(
    task: LoadProfile(),
    loading: () => new TextBlock().Text("Loading..."),
    then: (user) => new ProfileCard(user),
    error: (ex) => new TextBlock().Text($"Error: {ex.Message}").Foreground(Brushes.Red)
);
```

*   **task**: The `Task<T>` to await.
*   **loading**: (Optional) View to show while the task is running.
*   **then**: Function to build the Success view using the result of the task.
*   **error**: (Optional) Function to build the Error view if the task throws an exception.
