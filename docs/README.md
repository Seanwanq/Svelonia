# Svelonia Documentation

Welcome to the official documentation for Svelonia, a C# UI framework for Avalonia built with a focus on developer experience, fine-grained reactivity, and **Full Native AOT compatibility**.

## Core Pillars

- **Pure C#**: No XAML, no magic strings.
- **Fine-Grained Reactivity**: State-driven UI with automatic dependency tracking.
- **Zero-Reflection**: Optimized for Native AOT and Aggressive Trimming.
- **Fluent API**: Chainable, discoverable, and type-safe.

## Modules

*   **[Svelonia.Core](./Svelonia.Core/README.md)**
    The heart of the framework. Contains the reactivity system (`State<T>`, `Computed<T>`), component primitives, and functional controls.

*   **[Svelonia.Fluent](./Svelonia.Fluent/README.md)**
    A fluent API wrapper around Avalonia controls to make UI composition concise and readable. Includes advanced **[Drag & Drop](./Svelonia.Fluent/DragDrop.md)** support.

*   **[Svelonia.Kit](./Svelonia.Kit/README.md)**
    The application kit containing the Router, NavigationHost, and page structure.

*   **[Svelonia.Data](./Svelonia.Data/README.md)**
    Data handling patterns including the Mediator implementation for clean architecture.
