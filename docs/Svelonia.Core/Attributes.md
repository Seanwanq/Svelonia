[← Back to Svelonia.Core](./README.md)

# Attributes

Attributes used to configure component behavior within the Svelonia framework.

## [Parameter]

Marks a property in a `Page` or `Component` as a receiver for Route Parameters or Query Strings.

### Usage

```csharp
using Svelonia.Core;

public class ProductPage : Page
{
    // Matches route: /product/{Id}
    [Parameter]
    public string Id { get; set; }

    // Matches query: /product/123?ref=marketing
    [Parameter]
    public string Ref { get; set; }
}
```

*   **Type Conversion**: The framework automatically attempts to convert string parameters to `int`, `long`, `double`, `bool`, and `Guid`.

---

## [KeepAlive]

Indicates that a component (typically a Page) should be cached by the Router. When navigating away and back, the **same instance** will be reused, preserving its internal state and scroll position.

### Usage

```csharp
using Svelonia.Core;

[KeepAlive]
public class HeavyChartPage : Page
{
    // This constructor runs only once per session
    public HeavyChartPage() 
    {
        // Expensive initialization
    }
}
```

### Behavior
*   **Without KeepAlive**: The component is Disposed when navigating away. A new instance is created when navigating back.
*   **With KeepAlive**: The component remains in memory.
