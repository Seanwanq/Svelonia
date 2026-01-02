# Svelonia

A C# UI framework for Avalonia built with a focus on developer experience, fine-grained reactivity, and fluent APIs. Inspired by Svelte and SolidJS.

## Features

- **Fine-Grained Reactivity**: Accurate updates without a Virtual DOM or full re-renders.
- **Fluent UI**: Build interfaces entirely in C# with a chainable, type-safe API.
- **File-based Routing**: Automatic route discovery and parameter injection.
- **Clean Architecture**: Built-in Mediator pattern and DI support.
- **AOT Ready**: Optimized for Native AOT.

## Getting Started

### Installation

Install the Svelonia project templates:

```bash
dotnet new install Svelonia.Templates
```

### Create a New Project

Create a standard Svelonia application:

```bash
dotnet new svelonia.app -n MyApp
```

### Native AOT Support

Create a project pre-configured for Native AOT:

```bash
dotnet new svelonia.app -n MyAotApp --Aot
```

## Documentation

Comprehensive documentation is available in the [docs](./docs/README.md) folder.

- [Svelonia.Core](./docs/Svelonia.Core/README.md)
- [Svelonia.Fluent](./docs/Svelonia.Fluent/README.md)
- [Svelonia.Kit](./docs/Svelonia.Kit/README.md)
- [Svelonia.Data](./docs/Svelonia.Data/README.md)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
