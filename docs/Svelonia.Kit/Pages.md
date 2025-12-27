[← Back to Svelonia.Kit](./README.md)

# Pages

`Page` is a specialized `Component` that serves as a route target.

## Definition

Inherit from `Svelonia.Kit.Page`.

```csharp
using Svelonia.Kit;

public class HomePage : Page
{
    public HomePage()
    {
        Title = "Home";
        Content = new TextBlock().Text("Welcome!");
    }
}
```

## Properties

| Property | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | The title of the page. Can be used by layouts to update the window title. |

## Lifecycle

### OnLoadAsync

Called when the route parameters are ready. This is where you should fetch data.

```csharp
public override async Task OnLoadAsync(RouteParams p)
{
    if (p.TryGetValue("id", out var id))
    {
        // Load data
    }
}
```

### CanLeaveAsync

Called before the router navigates away from this page. Return `false` to cancel navigation (e.g., if there are unsaved changes).

```csharp
public override async Task<bool> CanLeaveAsync()
{
    if (HasUnsavedChanges)
    {
        // Show confirmation dialog...
        return await ConfirmDiscardAsync();
    }
    return true;
}
```

*(See `Component` documentation for standard lifecycle methods like `OnAttachedToVisualTree`)*