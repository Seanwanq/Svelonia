# Svelonia App Template

Welcome to your new Svelonia application! This project is pre-configured with skeuomorphic styling, reactive state management, and a convention-based routing system.

## Features

- **Pure C# UI**: No XAML required. Build your UI using a fluent, chainable API.
- **Reactive Core**: Powered by `State<T>` and `Computed<T>` for seamless data binding.
- **Convention Routing**: Pages are automatically registered based on your folder structure.
- **Skeuomorphic Theming**: Pre-configured with gradients and box shadows for a modern, tactile look.
- **Hot Reload**: Fast development cycle with built-in hot reload support.

## Getting Started

1.  **Themes**: Check `Themes/light.json` and `Themes/dark.json` to customize your application's colors and effects.
2.  **Pages**: Add new `.cs` files to the `Pages` directory to create new routes.
3.  **Styles**: Use `.BoxShadow(Sve.Res("..."))` and `.Background(Sve.Res("..."))` to apply themed visuals.

## Run the App

```bash
dotnet run
```

## Build for Release

```bash
dotnet build -c Release
```
