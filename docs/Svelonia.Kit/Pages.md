[← Back to Svelonia.Kit](./README.md)

# Pages & Hosting

## Page

The `Page` class extends `Svelonia.Core.Component` and adds routing-specific lifecycle methods.

### OnLoadAsync

This method is called immediately after the page is instantiated but before it is fully displayed. It is the ideal place to fetch initial data based on route parameters.

```csharp
public class ProductPage : Page
{
    private State<Product?> _product = new(null);

    public override async Task OnLoadAsync(RouteParams p)
    {
        var id = p["id"];
        _product.Value = await Api.GetProduct(id);
    }
}
```

### Lifecycle
1.  **Constructor**: Initialize UI structure (skeleton).
2.  **OnLoadAsync**: Fetch data.
3.  **AttachedToVisualTree**: Component is on screen.
4.  **DetachedFromVisualTree**: Component is removed (Disposed if not KeepAlive).

## NavigationHost

The `NavigationHost` is the UI control that acts as the placeholder for the current page. It observes the `Router.CurrentView` state.

```csharp
// In your MainWindow or MainLayout
Content = new NavigationHost(App.Router);
```

It automatically handles switching views when navigation occurs.
