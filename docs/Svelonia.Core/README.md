[← Back to Main Index](../README.md)

# Svelonia.Core

The `Svelonia.Core` library provides the fundamental building blocks for Svelonia applications. It introduces a fine-grained reactivity system inspired by frameworks like SolidJS and Svelte, but implemented fully in C#.

## Key Concepts

*   **Reactivity**: Managing state changes efficiently without full UI re-renders.
*   **Components**: The base unit of composition, extending Avalonia's `UserControl` with lifecycle management.
*   **Functional Controls**: Logic-as-UI components like `IfControl` and `AwaitControl`.

## Application Setup

Svelonia provides extensions to simplify the `App.axaml.cs` initialization logic.

### Loading Styles
Instead of imperative static calls, you can use the fluent `LoadStyles` method in your `Initialize` override:

```csharp
public override void Initialize()
{
    Styles.Add(new FluentTheme());
    
    // Load your Svelonia styles fluently
    this.LoadStyles(
        Test3.Styles.Styles.Load,
        Test3.Styles.Transitions.Load
    );
}
```

## Table of Contents

1.  **[State Management](./State.md)**
    Learn how to use `State<T>`, `Computed<T>`, and `StateList<T>` to drive your UI.

2.  **[Components & Layout](./Components.md)**
    Understanding the `Component` lifecycle, `Layout` templates, and resource management.

3.  **[Functional Controls](./Controls.md)**
    Using `If` and `Await` to handle conditional rendering and async operations declaratively.

4.  **[Animation](./Animation.md)**
    Built-in transition primitives for entering and leaving elements.

5.  **[Attributes](./Attributes.md)**
    Metadata attributes like `[Parameter]` and `[KeepAlive]` for configuring component behavior.

6.  **[Theming System](./Theming.md)**
    A JSON-based theme engine with auto-type conversion and hot-swapping support.

7.  **[Services](./Services.md)**
    Core abstractions like `IDialogService`.

8.  **[Debugging & Hot Reload](./Debugging.md)**
    Tools for monitoring state changes and hooking into the Hot Reload pipeline.