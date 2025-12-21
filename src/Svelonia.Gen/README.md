# Svelonia.Gen

The magic behind Svelonia. This library contains Roslyn Source Generators that automate repetitive tasks.

## What it Generates

1.  **Routes**: Scans your `Pages` directory and generates the `RouteRegistry`.
2.  **Fluent Extensions**: Scans Avalonia controls and generates fluent methods for properties.
3.  **Mediator Registry**: Scans for `IRequestHandler` implementations and generates AOT-safe DI registration code.

## Documentation

This is an infrastructure package. Its effects are documented in the respective modules it supports:

*   **Routing**: See [Svelonia.Kit Documentation](../../docs/Svelonia.Kit/README.md).
*   **Fluent API**: See [Svelonia.Fluent Documentation](../../docs/Svelonia.Fluent/README.md).
*   **Mediator**: See [Svelonia.Data Documentation](../../docs/Svelonia.Data/README.md).
