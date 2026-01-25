# Svelonia.Controls

`Svelonia.Controls` is an advanced component library for Svelonia, providing high-performance solutions for complex UI scenarios like infinite canvases, virtualization, and advanced graphics.

## Components

### 1. InfiniteCanvas

A container control that supports GPU-accelerated Panning and Zooming using a Matrix transform.

#### Key Features
*   **Flicker-Free**: Uses `RenderTransform` instead of `LayoutTransform`, avoiding heavy layout passes during zoom.
*   **Mouse-Centered Zoom**: Automatically compensates offset to keep the zoom anchor under the cursor.
*   **Touch & Trackpad Optimized**: Built-in support for two-finger panning and high-precision scrolling.
*   **Infinite World**: Supports arbitrary coordinates (positive and negative).

#### Usage
```csharp
var canvas = new InfiniteCanvas();

// The content can be any control (usually a Canvas or Panel)
canvas.Content = new Canvas()
    .MapToChildren(_visibleItems, item => RenderItem(item));

// You can control the view programmatically
canvas.ViewMatrix.Value = Matrix.CreateTranslation(100, 100);
```

### 2. ReactiveViewport

A powerful utility for implementing **Reactive Culling** (Manual Virtualization). It calculates which items from a large dataset should be rendered based on the current `InfiniteCanvas` view.

#### Key Features
*   **Throttled Calculation**: Built-in thresholds for movement and scale changes to minimize CPU churn.
*   **Data Aware**: Automatically invalidates the cache if the source data instance changes.
*   **Flexible Bounds**: Supports AABB (Axis-Aligned Bounding Box) intersection checks.

#### Usage
```csharp
var visibleItems = ReactiveViewport.CreateVisibleSet(
    canvas: _infiniteCanvas,
    source: _allData, // State<IEnumerable<T>>
    boundsSelector: item => item.Bounds,
    moveThreshold: 50,   // Only re-filter if moved > 50px
    scaleThreshold: 0.1  // Only re-filter if zoomed > 10%
);

// Bind result to your UI
nodeCanvas.MapToChildren(visibleItems, node => RenderNode(node));
```

### EnsureVisible
Smoothly pans the view to bring a specific world-coordinate rectangle into focus.

```csharp
// Simple uniform padding
_canvas.EnsureVisible(node.Bounds, padding: 100);

// Asymmetric padding (e.g., provide extra room for a top toolbar)
_canvas.EnsureVisible(node.Bounds, padding: new Thickness(100, 250, 100, 100));
```

---

## Utilities
