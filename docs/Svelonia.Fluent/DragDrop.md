# Drag & Drop Deep Dive

Svelonia provides a powerful, semantic Drag & Drop API designed for modern UIs. We categorize Drag & Drop into two distinct systems: **Standard (`Draggable`)** and **Real-Time (`LiveDraggable`)**.

---

## 1. Conceptual Model: Standard vs. Live

| Feature | Standard `Draggable` | Real-Time `LiveDraggable` |
| :--- | :--- | :--- |
| **Implementation** | OS OLE Loop (Blocking) | Pointer Capture + RenderTransform (Non-blocking) |
| **Concurrency** | Single object at a time | **Multi-touch concurrent dragging** |
| **Scope** | Cross-window / Cross-process | Application internal only |
| **Visuals** | Static Snapshot (Ghost Image) | **Fully Interactive UI (Live Ghost)** |
| **Best For** | Data exchange, List sorting | Whiteboards, Node editors, Games |

---

## 2. Standard Drag & Drop (`Draggable`)

### Basic Example
```csharp
var items = new State<List<string>>(new() { "Item 1", "Item 2" });

// Source
new Border().Draggable(data: "Item 1", effect: DragDropEffects.Move);

// Target
new Border().OnDrop(e => {
    var data = e.Data.GetText();
    // Update your state here
});
```

### Advanced: Ghost Customization & Logic
```csharp
.Draggable(
    data: myModel,
    visualMode: DragVisualMode.Move, // Hides original during drag
    animateBack: true,               // Returns to start if dropped outside
    ghostTransform: img => new StackPanel().Children(
        img, 
        new TextBlock().TextContent("Moving...").Foreground(Brushes.White)
    ),
    onStart: () => Console.WriteLine("Drag Started"),
    onEnd: result => {
        if (result == DragDropEffects.Move) { /* Handle Success */ }
    }
)
```

---

## 3. Real-Time Dragging (`LiveDraggable`)

### Basic Example
```csharp
new Border().W(100).H(100).Background(Brushes.Blue)
    .LiveDraggable(animateBack: true);
```

### Advanced: Constraints and Live Ghosts
```csharp
var ballPos = new State<Point>(default);

new Border()
    .LiveDraggable(
        handle: myGripControl,           // Drag only via handle
        constrainTo: myBoundaryPanel,    // Clamp to parent boundaries
        boundaryMode: LiveDragBoundaryMode.Clamp, 
        ghostTransform: _ => new MyComplexOverlay(ballPos), // Reactive top-layer UI
        onMove: p => ballPos.Value = p   // Update state for reactive bindings
    );
```

---

## 4. Drop Target Interactivity

To create a professional feel, provide feedback when an item is hovered over a target.

```csharp
var isOver = new State<bool>(false);

new Border()
    .Background(new Computed<IBrush>(() => isOver.Value ? Brushes.LightGreen : Brushes.White))
    .OnDragEnter(e => isOver.Value = true)
    .OnDragLeave(e => isOver.Value = false)
    .OnDrop(e => {
        isOver.Value = false;
        // Process data...
    });
```

---

## 5. State Management Patterns

The most common pattern is updating a list state upon a successful move.

```csharp
.Draggable(
    data: item,
    effect: DragDropEffects.Move,
    onEnd: result => {
        if (result == DragDropEffects.Move) {
            sourceList.Value = sourceList.Value.Where(x => x != item).ToList();
        }
    }
)
```

---

## 6. API Reference

### `Draggable<T>`
| Parameter | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `data` | `object` | **Required** | The payload to be transferred. |
| `effect` | `DragDropEffects` | `Copy` | Allowed operations (Copy, Move, Link). |
| `handle` | `Control?` | `null` | Optional trigger element. |
| `format` | `string?` | `null` | Custom data format string. |
| `enable` | `State<bool>?` | `null` | Reactive toggle for dragging. |
| `visualMode` | `DragVisualMode` | `Ghost` | `None`, `Ghost` (dimmed), or `Move` (hidden). |
| `ghostTransform`| `Func<Image, Control>?`| `null` | Customize the static drag snapshot. |
| `animateBack` | `bool` | `true` | Snap back animation on cancellation. |
| `onStart` | `Action?` | `null` | Callback when drag begins. |
| `onEnd` | `Action<DragDropEffects>?`| `null` | Callback when drag ends with result. |

### `LiveDraggable<T>`
| Parameter | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `handle` | `Control?` | `null` | Optional trigger element. |
| `enable` | `State<bool>?` | `null` | Reactive toggle for dragging. |
| `animateBack` | `bool` | `true` | Snap back animation on release. |
| `ghostTransform`| `Func<T, Control>?` | `null` | Create a reactive top-layer UI overlay. |
| `constrain` | `Func<Point, Point>?` | `null` | Custom math logic for position (e.g., Grid). |
| `constrainTo` | `Control?` | `null` | Constraint container (e.g., Parent Border). |
| `boundaryMode` | `LiveDragBoundaryMode`| `Clamp` | `Clamp` (block) or `Clip` (visual cut-off). |
| `onStart` | `Action?` | `null` | Callback when drag begins. |
| `onEnd` | `Action?` | `null` | Callback when drag ends. |
| `onMove` | `Action<Point>?` | `null` | Real-time offset (Delta) callback. |

---

## 7. Tips for Professional UIs

1. **Clip vs. Clamp**: Use `Clamp` for objects that must stay within a zone (e.g., map icons). Use `Clip` for objects that belong to a scrollable area or list.
2. **Transparent Hit-Testing**: Remember that `Brushes.Transparent` blocks mouse hits, while `null` (no background) allows them to pass through.
3. **Ghost Performance**: Keep `Live Ghost` trees lightweight. Since they move every frame, heavy layouts inside the ghost may impact smoothness.