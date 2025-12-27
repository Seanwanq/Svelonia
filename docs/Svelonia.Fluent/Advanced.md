[← Back to Svelonia.Fluent](./README.md)

# Advanced Topics

This section covers advanced usage scenarios for Svelonia.Fluent, including extending the framework to support third-party libraries.

## Supporting Third-Party Controls

Svelonia relies on C# Source Generators to create the fluent extension methods (like `.Width()`, `.Text()`, etc.) for standard Avalonia controls. However, you might often use third-party control libraries (such as `Semi.Avalonia`, `Material.Avalonia`, or your own custom controls) that Svelonia doesn't know about by default.

To enable the fluent API for these external controls, you can use the `[GenerateFluentExtensionsFor]` attribute.

### How to Enable

Add the `[GenerateFluentExtensionsFor]` assembly-level attribute anywhere in your project (e.g., in `Program.cs` or a dedicated `AssemblyAttributes.cs` file).

**1. Generate for a Single Type**

```csharp
using Svelonia.Fluent;
using ThirdParty.Library;

// Generates fluent extensions only for the 'ColorPicker' control
[assembly: GenerateFluentExtensionsFor(typeof(ColorPicker))]
```

**2. Generate for an Entire Namespace**

If a library has many controls, you can tell the generator to scan the namespace of a specific type and generate extensions for *all* controls found in that namespace.

```csharp
using Svelonia.Fluent;
using ThirdParty.Library.Controls;

// Generates extensions for 'FancyButton' AND all other controls 
// found in the same namespace as 'FancyButton'.
[assembly: GenerateFluentExtensionsFor(typeof(FancyButton), scanNamespace: true)]
```

### Usage

Once the attribute is added, the Source Generator will automatically create the extension methods. You can then use the third-party controls just like native ones:

```csharp
new ColorPicker()
    .Color(Colors.Red)
    .OnColorChanged(e => { ... })
    .Margin(10);
```

### Notes

- The generated code is `internal`, meaning it is visible only within the project where you added the attribute.
- The generator looks for classes that inherit from `Avalonia.AvaloniaObject`.
- Generic types are currently skipped to simplify generation.
