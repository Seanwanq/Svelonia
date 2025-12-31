# Drag & Drop Deep Dive

Dragging is more than just data transfer; it's about the visual sense of "control." Svelonia provides two distinct APIs to cover everything from simple data movement to complex real-time interactions.

## 1. Conceptual Model: Standard vs. Live

| Dimension | System-Level `Draggable` (Standard) | Real-Time `LiveDraggable` (Live) |
| :--- | :--- | :--- |
| **Implementation** | OS OLE / DragDrop Loop | Pointer Capture + RenderTransform |
| **Blocking** | Modal (blocks other interactions) | Non-modal (fully interactive) |
| **Concurrency** | Single object | **Multi-touch concurrent** |
| **External** | Supports cross-app (e.g., to Desktop) | Application internal only |
| **Best For** | File uploads, form filling, reordering | Mind maps, whiteboards, game objects |

---

## 2. System-Level Dragging (`Draggable`)

### A. Practical: Cross-Container Movement (State-Driven)
This is the most common use case: moving an item from a "Source" list to a "Target" list.

```csharp
// Define your state
var sourceList = new State<IEnumerable<string>>(new[] { "Item 1", "Item 2" });
var targetList = new State<IEnumerable<string>>(new string[0]);

// Source container rendering
new StackPanel()
    .Children(new ForControl(sourceList, item => 
        new Border().TextContent(item)
            .Draggable(data: item, effect: DragDropEffects.Move) // Transfer the item string
    ));

// Target container rendering
new Border()
    .OnDrop(e => {
        var item = e.Data.GetText();
        if (item != null) {
            // Reactive update: UI refreshes automatically
            sourceList.Value = sourceList.Value.Where(x => x != item);
            targetList.Value = targetList.Value.Append(item);
        }
    });
```

### B. Advanced Customization: "Adding Flavor" to the Ghost (Ghost Transform)
When dragging a complex card, you might want the visual representation to show extra info (like dragging status or real-time coordinates).

The `ghostTransform` hook gives you the auto-generated `Image` (a snapshot of the control taken at the moment of dragging) and expects a `Control` back. You can wrap this image in a `Border`, `StackPanel`, or even a `Canvas` to add custom UI elements that follow the mouse.

```csharp
.Draggable(
    data: myModel,
    visualMode: DragVisualMode.Move, // Original object is hidden
    ghostTransform: img => new StackPanel()
        .Spacing(5)
        .Children(
            // 1. The original snapshot of the card
            img.Opacity(0.8).Effect(new DropShadowEffect { BlurRadius = 10 }), 
            // 2. A dynamic "status" layer added on top/bottom
            new Border()
                .Background(Brushes.DarkRed)
                .Padding(5).Rounded(4)
                .Child(new TextBlock().TextContent("Moving to new group...").Foreground(Brushes.White))
        )
)
```

### C. Real-Time Layout Reflow (OnDrag)
During the drag operation, use the `onDrag` callback to notify other elements to "make room."

```csharp
.Draggable(
    data: "NODE_X",
    onDrag: globalPos => {
        // globalPos is the real-time mouse position relative to the Window
        if (CheckOverlap(globalPos, targetArea)) {
            isAreaHighlighted.Value = true;
            TriggerReflowLayout(); // Trigger reflow animations for other nodes
        }
    }
)
```

---

## 3. Real-Time Interactive Dragging (`LiveDraggable`)

### A. Mind Map Style Free Movement
`LiveDraggable` uses `RenderTransform`, meaning it doesn't trigger the layout system (like StackPanel re-measurement), making it perfect for free-form canvases.

```csharp
new Canvas()
    .Children(
        new Border()
            .W(100).H(50).Background(Brushes.SlateBlue)
            .LiveDraggable() // Each node can be dragged independently and concurrently
            .Left(50).Top(50), 
            
        new Border()
            .W(100).H(50).Background(Brushes.DarkCyan)
            .LiveDraggable() 
            .Left(200).Top(100)
    )
```

### B. Constraints and Synchronization (OnMove)
Sometimes you need to restrict movement, such as "snap to grid" or "sync resizing."

```csharp
.LiveDraggable(onMove: delta => {
    // delta is the total offset since the pointer was pressed
    // Implement snapping:
    var snappedX = Math.Round(delta.X / 20) * 20;
    // ... logic to apply snapped values
});
```

---

## 4. Troubleshooting

### Ghost image frozen or missing?
*   **Cause 1**: Mouse entered an area where `AllowDrop = true` is not set.
*   **Fix**: Svelonia has a built-in "Global Keep-alive" mechanism, but ensure your `TopLevel` (Window) is capable of receiving events.
*   **Cause 2**: You removed the original object in the `onStart` callback.
*   **Fix**: The snapshot is taken right after `onStart`. If you remove the object physically from the tree in `onStart`, the snapshot will be empty. Use `visualMode: DragVisualMode.Move` to hide it visually instead of deleting it.

### Drag handle not working?
If you specified a `handle` parameter, ensure that the handle control has `IsHitTestVisible` set to `true` (default) and is not obscured by a higher transparent layer.

---

## 5. API Reference

### `Draggable` Parameters
| Parameter | Type | Description |
| :--- | :--- | :--- |
| `data` | `object` | **Required**. The payload to transfer (String, Files, or any Object). |
| `effect` | `DragDropEffects` | Allowed operations (Copy, Move, Link). |
| `handle` | `Control` | Optional child control to act as the drag grip. |
| `enable` | `State<bool>` | Reactive state to toggle dragging capability. |
| `visualMode` | `DragVisualMode` | `Ghost` (dimmed source), `Move` (hidden source), or `None`. |
| `ghostTransform` | `Func<Image, Control>` | **Pro**. Function to wrap or replace the auto-generated ghost image. Useful for adding borders, status bars, or animations. |
| `onStart` | `Action` | Triggered when drag starts. |
| `onDrag` | `Action<Point>` | **Pro**. Real-time global coordinate callback during drag. |
| `onEnd` | `Action` | Triggered when drag ends (dropped or cancelled). |

### `LiveDraggable` Parameters
| Parameter | Type | Description |
| :--- | :--- | :--- |
| `enable` | `State<bool>` | Reactive state to toggle interaction. |
| `onMove` | `Action<Point>` | Callback providing the total translation delta from the start point. |

### Helper Methods
| Method | Description |
| :--- | :--- |
| `.OnDrop(Action<DragEventArgs>)` | Configures a control as a valid drop target. |
| `.OnDragEnter/Leave/Over` | Low-level event hooks for visual feedback. |
| `.ManualMove(x, y)` | Manually updates the `RenderTransform` of a control (used for sync/live-drag). |