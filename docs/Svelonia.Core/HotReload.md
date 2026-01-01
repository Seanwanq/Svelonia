# Hot Reload Workflow

Svelonia is designed to leverage C# Hot Reload to provide a rapid, feedback-driven development experience. Since your entire UI is defined in C#, you can often see changes instantly without restarting the application.

---

## 1. Supported Tools

- **Visual Studio 2022**: Supports "Hot Reload on Save" for most C# changes.
- **VS Code**: Use `dotnet watch` from the terminal.
- **JetBrains Rider**: Use the built-in Hot Reload support.

---

## 2. Using `dotnet watch` (Recommended)

Run your project using the following command in your terminal:

```bash
dotnet watch run
```

When you save a file, Svelonia's `HotReloadManager` works with Avalonia to re-render the current Page while attempting to preserve as much state as possible.

---

## 3. What can be Hot Reloaded?

### ✅ Supported (Instant Refresh)
- Changing layout structure (e.g., swapping a `StackPanel` for a `Grid`).
- Updating styling properties (e.g., changing `.Bg(Brushes.Blue)` to `.Bg(Brushes.Red)`).
- Adding or removing controls.
- Modifying `Computed` logic.

### ⚠️ Limited (Requires Page Re-navigation)
- Modifying the constructor logic of a `Page`. Svelonia will try to re-instantiate the page, but complex constructor side-effects might require manual navigation.

### ❌ Not Supported (Requires Restart)
- Adding new `[Parameter]` properties to a Page (requires Source Generator re-run).
- Changing class names or inheritance.
- Modifying `Program.cs` setup logic.

---

## 4. State Preservation

Svelonia attempts to preserve state during Hot Reload using the following strategies:

1.  **KeepAlive**: Pages marked with `[KeepAlive]` are more likely to restore their scroll position and local `State<T>` values.
2.  **Global Stores**: If you keep your application state in Singleton services (Stores), Hot Reload will NOT clear this data. This is the **most robust** way to develop with Hot Reload.

---

## 5. Troubleshooting

If Hot Reload fails to reflect your changes:
1.  Check the terminal output for "Hot reload compilation failed".
2.  Perform a manual navigation (e.g., click a link to another page and back).
3.  Restart the `dotnet watch` process if the Source Generator output is out of sync.
