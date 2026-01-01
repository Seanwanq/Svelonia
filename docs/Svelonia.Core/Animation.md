[← Back to Svelonia.Core](./README.md)

# Animation & Motion

Svelonia makes animations declarative. There are two primary ways to animate: **Implicit Transitions** (for property changes) and **Control Transitions** (for adding/removing elements).

---

## 1. Implicit Transitions (`Animate`)

The simplest way to add polish is the `.Animate()` helper. It adds a set of standard Avalonia transitions to the control, making property changes smooth.

```csharp
new Border()
    .Animate() // Enables smooth transitions for Background, Opacity, and CornerRadius
    .Bg(Brushes.Blue, hover: Brushes.Red)
    .Rounded(4, hover: 12);
```

### Under the Hood: `trans-all`
`.Animate()` is a shorthand for adding the `.WithClass("trans-all")` CSS class. You can define this class in your global styles to control exactly which properties animate by default.

---

## 2. Control Transitions (`IfControl` & `Router`)

When elements enter or leave the visual tree (e.g., inside an `IfControl` or during Page navigation), use the `Transition` class.

### Standard Presets
- `Transition.Fade(duration)`
- `Transition.Fly(duration, x, y)`
- `Transition.Scale(duration, scale)`

### Customizing Easings
All presets accept an `Easing` parameter (using standard Avalonia easing functions).

```csharp
using Avalonia.Animation.Easings;

Transition.Fly(500, y: 20, easing: new ExponentialEaseOut());
```

---

## 3. Deep Customization: `SveloniaTransition`

For complex sequences, you can define your own `SveloniaTransition`. This allows you to orchestrate multiple properties that are not part of standard presets.

```csharp
var myEffect = new SveloniaTransition
{
    Duration = TimeSpan.FromMilliseconds(600),
    // State before the element enters
    ApplyInitialState = ctrl => {
        ctrl.Opacity = 0;
        ctrl.SetRotate(45); // Using fluent extension for RotateTransform
    },
    // Target state
    ApplyActiveState = ctrl => {
        ctrl.Opacity = 1;
        ctrl.SetRotate(0);
    },
    // Tells Avalonia which properties to animate
    CreateTransitions = () => new Transitions {
        new DoubleTransition { Property = Visual.OpacityProperty, Duration = TimeSpan.FromMilliseconds(600) },
        new DoubleTransition { Property = RotateTransform.AngleProperty, Duration = TimeSpan.FromMilliseconds(600), Easing = new BackEaseOut() }
    }
};
```

---

## 4. Staggered Animations (Best Practice)

When rendering lists with `Sve.Each`, you can create a staggered entry effect by using a small index-based delay in your transition logic (requires manual implementation in the builder).

```csharp
Sve.Each(items, (item, index) => 
    new MyItemView(item)
        .SetOpacity(0)
        .Animate() // Or use a custom transition logic triggered by AttachedToVisualTree
);
```

> **Tip**: For high-performance animations (like 60fps games or complex data visualizations), consider using the `Composition` layer directly, which Svelonia supports via standard Avalonia interop.
