# Reactive Control Flow

Svelonia provides dedicated components to handle conditional rendering, lists, and asynchronous data, keeping your UI code declarative.

## Conditional Rendering (`Sve.If`)

Instead of manual `if` statements that modify the visual tree, use `Sve.If` to reactively toggle elements.

```csharp
var isLoggedIn = new State<bool>(false);

Content = Sve.If(isLoggedIn,
    () => new TextBlock().SetText("Welcome back!"),
    elseBuilder: () => new Button().SetContent("Log In")
);
```

## Lists and Collections (`Sve.Each`)

`Sve.Each` is optimized for rendering `StateList<T>`. It performs incremental updates, meaning it only creates controls for new items and moves existing ones, preserving their state (like focus).

```csharp
var todos = new StateList<TodoItem>();

Content = new StackPanel().SetChildren(
    Sve.Each(todos, todo => new TodoView(todo))
);
```

## Asynchronous Data (`Sve.Await`)

Handle `Task<T>` directly in your UI. `Sve.Await` manages the loading and error states for you.

```csharp
Task<User> userInfoTask = FetchUserAsync();

Content = Sve.Await(userInfoTask,
    then: user => new TextBlock().SetText($"Hello, {user.Name}"),
    loading: () => new ProgressBar().SetIsIndeterminate(true),
    error: ex => new TextBlock().SetText($"Error: {ex.Message}").Fg(Brushes.Red)
);
```