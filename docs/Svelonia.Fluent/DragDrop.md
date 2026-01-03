# Drag and Drop

Svelonia provides two drag-and-drop systems: a standard one for inter-app data transfer, and a high-performance **Live Drag** system for interactive UIs like mind-maps.

## Live Drag (`LiveDraggable`)

This system is designed for elements that move in real-time with the pointer.

```csharp
new Border()
    .LiveDraggable(
        enable: new State<bool>(true),
        onStart: () => Log("Started"),
        constrain: (point) => {
            // Snap to grid of 20
            return new Point(
                Math.Round(point.X / 20) * 20, 
                Math.Round(point.Y / 20) * 20
            );
        },
        onDelta: (delta) => UpdateLogic(delta)
    );
```

### Key Features
- **Zero Latency**: Direct pointer-to-transform mapping.
- **Constrain Function**: Perfectly implement snapping, boundaries, or axis-locking.
- **State Aware**: Can be enabled/disabled via reactive states.

## Standard Drag and Drop

Used for transferring data between controls or applications.

```csharp
// Source
new Border().Draggable(data: "Some Data", effect: DragDropEffects.Move);

// Target
new Panel().OnDrop(e => {
    var data = e.Data.GetText();
    HandleDrop(data);
});
```
