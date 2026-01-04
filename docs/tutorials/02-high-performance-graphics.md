# Building a High-Performance MindMap

This tutorial explains the high-performance graphics architecture used in the **AvaXMind** demo.

## 1. MatrixTransform over LayoutTransform

Standard Avalonia UI uses `LayoutTransform` for scaling. However, `LayoutTransform` triggers a full **Layout Pass** (Measure/Arrange) on every frame, which is expensive for large diagrams.

### The Strategy
Use `RenderTransform` with a `Matrix`.
1.  **Why**: It is processed entirely on the GPU. No CPU-side layout is recalculated during zoom/pan.
2.  **How**: Use the `InfiniteCanvas` component from `Svelonia.Controls`. It encapsulates the matrix math.

## 2. Reactive Culling (Visual Virtualization)

Even with GPU transforms, rendering 10,000 controls is too slow. We must "cull" (remove) items that are off-screen.

### The Algorithm: Inverse Mapping
1.  **View Space**: Your screen (e.g., 800x600).
2.  **World Space**: Your infinite canvas (e.g., coordinates at 25000, 25000).
3.  **The Trick**: Invert your `ViewMatrix` to find out which "World Rect" currently maps to your "Screen Rect".

```csharp
// Simplified Logic
var inv = ViewMatrix.Invert();
var worldViewport = screenRect.Transform(inv);

// Only render nodes that intersect worldViewport
var visible = allNodes.Where(n => worldViewport.Intersects(n.Bounds));
```

### Edge Culling (AABB)
Lines crossing the screen must be visible even if their start and end nodes are off-screen.
*   **Wrong**: `if (IsVisible(Start) || IsVisible(End))`
*   **Correct**: Calculate the **Bounding Box (AABB)** of the line and check if *that box* intersects the viewport.

## 3. Incremental Layout

Don't recalculate the entire tree when one node moves.
*   **Decentralized**: Each node calculates its own subtree size (`BranchHeight`) and positions its immediate children.
*   **Reactive Propagation**: A change in a leaf node "bubbles up" only through its parent chain, minimizing the calculation cost to **O(Log N)**.

---

By combining **Matrix Transforms**, **Reactive Culling**, and **Incremental Layout**, you can build diagrams that handle 100,000+ nodes with 60fps performance on any device.
