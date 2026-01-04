# Svelonia Documentation

Welcome to the Svelonia documentation. Svelonia is a C# UI framework for Avalonia that brings Svelte-like DX to desktop development.

## Core Concepts

- **[State Management](./Svelonia.Core/State.md)**: Fine-grained reactivity with `State`, `Computed`, and `Effect`.
- **[Fluent UI](./Svelonia.Fluent/Basics.md)**: Build interfaces entirely in C# with a chainable API.
- **[Advanced Controls](./Svelonia.Controls/README.md)**: Infinite Canvas, Reactive Viewport, and high-performance graphics.
- **[Styling](./Svelonia.Fluent/Styling.md)**: Declarative styles with `WhenHovered`, `WhenPressed`, and atomic helpers.
- **[Bindings & Events](./Svelonia.Fluent/Bindings.md)**: Type-safe bindings and Universal Event API (`.OnXXX`).
- **[Animations](./Svelonia.Core/Animation.md)**: Smooth transitions with `.Animate()`.

## 🎓 Learning Path

If you want to master Svelonia beyond the basic API, check out our deep-dive tutorials:

- **[Svelonia Tutorials Index](./tutorials/README.md)**
- [01. Reactivity Mastery](./tutorials/01-reactivity-fundamentals.md)
- [02. Building High-Performance Diagrams](./tutorials/02-high-performance-graphics.md)
- [03. Custom Component Deep Dive](./tutorials/03-component-deep-dive.md)

## Quick Start Example

```csharp
public class CounterPage : Page
{
    public CounterPage()
    {
        var count = new State<int>(0);
        var label = new Computed<string>(() => $"Count is: {count.Value}");

        Content = new StackPanel()
            .P(4)
            .SetSpacing(10)
            .SetChildren(
                new TextBlock().BindText(label),
                new Button()
                    .SetContent("Increment")
                    .OnClick(() => count.Value++)
                    .WhenHovered(s => s.Bg(Brushes.Azure))
            );
    }
}
```

## Advanced Topics

- **[Navigation & Routing](./Svelonia.Kit/Routing.md)**: File-based routing and navigation hosts.
- **[Native AOT Support](./Svelonia.Core/AOT.md)**: How to optimize your app for Native AOT.
- **[A Mind-Map Demo (AvaXMind)](https://github.com/Seanwanq/Svelonia.PrivateDemo)**: A real-world example of complex interactions.