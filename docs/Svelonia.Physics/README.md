# Svelonia Physics Engine

Svelonia provides a lightweight, reactive physics engine optimized for UI animations. It is designed to handle high-load scenarios (10k+ active springs) with smooth, stable performance.

## Core Concepts

### SpringState

`SpringState` is a reactive wrapper around a physical spring simulation. It behaves like a standard `State<double>`, but when you assign a new value, it interpolates to that target using spring dynamics (Mass, Stiffness, Damping).

```csharp
// Create a spring at position 0
var x = new SpringState(0);

// Assign a new target
x.Value = 100; 

// Bind to UI
myControl.BindLeft(x); 
```

### Configuration

You can tune the physics properties directly on the state:

```csharp
x.Stiffness = 200; // Higher = Snappier
x.Damping = 20;    // Higher = Less oscillation
x.Mass = 1;        // Higher = Heavier/Slower
```

### Immediate Updates

If you need to "teleport" a value without animation (e.g., during initialization or drag start), use `SetImmediate`:

```csharp
x.SetImmediate(500); // Teleports to 500, clearing velocity
```

## Advanced Physics Integration

The engine uses a **Robust Sub-stepping (SOLA)** approach with **Semi-Implicit Euler** integration. This ensures:
1.  **Stability**: Springs won't "explode" or ghost even if the UI frame rate drops significantly (e.g., during heavy layout calculations).
2.  **Accuracy**: The simulation runs at a fixed internal timestep (1ms), ensuring consistent behavior across different devices and refresh rates.
3.  **Efficiency**: Sleeping springs (at rest) are automatically removed from the global `AnimationLoop` to save CPU.

### Reactive Targets

`SpringState` exposes a `TargetState` property. This is a `State<double>` representing the *final destination* of the spring.

*   `x.Value`: The current animated value (changes every frame).
*   `x.TargetState.Value`: The destination value (changes only when target is set).

**Best Practice**: Use `TargetState` for layout calculations (to avoid layout thrashing during animation) and `Value` for rendering.

```csharp
// Layout Logic: Use TargetState to determine where things "should" be
var totalHeight = children.Sum(c => c.Y.TargetState.Value);

// Render Logic: Use Value for smooth visual movement
renderTransform.X = node.X.Value;
```
