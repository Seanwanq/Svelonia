[← Back to Svelonia.Kit](./README.md)

# Routing

The `Router` is responsible for managing the application's URL state and deciding which component to render.

## Basics

The `Router` handles navigation. It must be instantiated and passed to the `NavigationHost`, which renders the current view.

```csharp
// App.cs
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var router = new Router();
        
        // NavigationHost is a UserControl, so wrap it in a Window
        desktop.MainWindow = new Window 
        { 
            Content = new NavigationHost(router) 
        };
        
        // Initial navigation
        router.Navigate("/");
    }
    base.OnFrameworkInitializationCompleted();
}
```

## Route Registration

Route registration in Svelonia is **convention-based** and handled by the `Svelonia.Gen` source generator. It uses your project's folder structure and class names to build the routing table.

### File Path to URL Mapping

The generator looks for classes inheriting from `Page` within the `Pages` namespace.

| File Path | Class Name | Generated Route |
| :--- | :--- | :--- |
| `Pages/IndexPage.cs` | `IndexPage` | `/` |
| `Pages/AboutPage.cs` | `AboutPage` | `/about` |
| `Pages/Auth/LoginPage.cs` | `LoginPage` | `/auth/login` |
| `Pages/Users/IndexPage.cs` | `IndexPage` | `/users` |
| `Pages/Store/CategoryPage.cs` | `CategoryPage` | `/store/category` |

**Rules:**
1.  **Index**: A class named `IndexPage` (or any `Index` suffix) at the root or in a subfolder represents the default route for that path.
2.  **Naming**: The `Page` suffix is automatically stripped.
3.  **Case Sensitivity**: Generated routes are lowercase.

---

## Route Parameters

To create a dynamic route (e.g., `/user/123`), use the `Param_` prefix in your class name.

### File Mapping for Parameters

| File Path | Class Name | Generated Route |
| :--- | :--- | :--- |
| `Pages/Users/Param_Id.cs` | `Param_Id` | `/users/{id}` |
| `Pages/Docs/Param_SlugPage.cs` | `Param_SlugPage` | `/docs/{slug}` |

### Automatic Parameter Injection

The most efficient way to use parameters is via the `[Parameter]` attribute. Svelonia's generator automatically injects values and performs basic type conversion.

```csharp
using Svelonia.Core;
using Svelonia.Kit;

namespace MyApp.Pages.Users;

public class Param_Id : Page
{
    // The framework finds "id" in the route and assigns it here.
    // It automatically handles int.TryParse for you.
    [Parameter]
    public int Id { get; set; }

    public override async Task OnLoadAsync(RouteParams p)
    {
        // At this point, Id is already populated.
        Console.WriteLine($"Loading user with ID: {Id}");
    }
}
```

**Supported Types for Auto-Injection:**
*   `string`, `int`, `long`, `double`, `bool`, `Guid`.

---

## Query Parameters

Query parameters (e.g., `?search=phone&page=1`) are automatically parsed and available in the `RouteParams` dictionary during `OnLoadAsync`.

### Example: Search Page

**URL**: `/search?q=avalonia&limit=10`

```csharp
public class SearchPage : Page
{
    [Parameter("q")] // Map query key "q" to this property
    public string Query { get; set; }

    [Parameter] // Key defaults to property name "limit"
    public int Limit { get; set; }

    public override async Task OnLoadAsync(RouteParams p)
    {
        // Manual access is also possible:
        bool hasSort = p.TryGetValue("sort", out var sortOrder);
        
        Console.WriteLine($"Searching for {Query}, Limit: {Limit}");
    }
}
```

### Navigating with Query Params

```csharp
// From code
Router.Navigate("/search?q=svelonia&limit=5");

// From UI (Fluent API)
new Button()
    .SetContent("Next Page")
    .OnClick(_ => Router.Navigate($"/search?q={Query}&limit={Limit}&page=2"));
```

## Page Lifecycle

Svelonia Pages have a deterministic lifecycle, even when using caching (`KeepAlive`).

- **OnLoadAsync**: Guaranteed to be called **every time** a page is navigated to, even if the page instance is being reused from cache. This is the ideal place to synchronize parameters with your application state.
- **CanLeaveAsync**: Called before the page is removed from view. Return `false` to cancel navigation.

### Example Lifecycle Flow

```csharp
public override async Task OnLoadAsync(RouteParams p)
{
    // This runs every time you click a link to this page,
    // regardless of whether the component was just created or cached.
    var id = p["id"];
    await LoadData(id);
}
```

---

## Navigation Guards

You can intercept navigation to prevent users from losing unsaved changes or accessing restricted areas.

### Page-Level Guard

Override `CanLeaveAsync` in your `Page` class. This is the preferred way to handle "Unsaved Changes" scenarios.

```csharp
public override async Task<bool> CanLeaveAsync()
{
    if (IsDirty)
    {
        return await ShowConfirmDialog("Discard changes?");
    }
    return true;
}
```

### Global Guard

Use `Router.AddGuard` to intercept *all* navigation attempts.

```csharp
router.AddGuard(async (path) => 
{
    if (path.StartsWith("/admin") && !User.IsAdmin)
    {
        return false; // Cancel navigation
    }
    return true;
});
```
