[← Back to Svelonia.Core](./README.md)

# Animation

Svelonia provides a set of standard transitions that can be used with `IfControl` or other state-switching components.

## Transition Class

Located in `Svelonia.Core.Animation`.

### Available Transitions

#### 1. Fade
Opacity transition from 0 to 1.

```csharp
var t = Transition.Fade(duration: 300);
```

#### 2. Fly
Combines Fade with a translation (movement).

```csharp
// Fly in from the bottom (Y + 20)
var t = Transition.Fly(duration: 300, y: 20);

// Fly in from the right
var t = Transition.Fly(duration: 300, x: 50);
```

#### 3. Scale
Combines Fade with a scaling effect.

```csharp
// Scale up from 95% to 100%
var t = Transition.Scale(duration: 300, startScale: 0.95);
```

### Custom Transitions

You can create custom transitions by instantiating `SveloniaTransition`:

```csharp
new SveloniaTransition
{
    Duration = TimeSpan.FromMilliseconds(500),
    // Setup initial properties (before animation starts)
    ApplyInitialState = ctrl => ctrl.Opacity = 0.5,
    // Setup active properties (target state)
    ApplyActiveState = ctrl => ctrl.Opacity = 1.0,
    // Define Avalonia Transitions
    CreateTransitions = () => new Transitions { ... }
};
```
