# Ahead-of-Time (AOT) Compilation

Svelonia is one of the few Avalonia-based frameworks built from the ground up for **Native AOT** and **Aggressive Trimming**. By replacing XAML and reflection-based bindings with source-generated C# code, Svelonia enables ultra-small binary sizes and near-instant startup times.

---

## 1. The Zero-Reflection Binding System

Standard Avalonia applications often rely on `new Binding { Path = "Property" }`, which uses runtime reflection to find data. This is incompatible with strict AOT.

**Svelonia's Solution:**
All `.BindX()` methods and reactive styles (like `.Bg(state)`) use a **Zero-Reflection path**:
- `IState` implements `IObservable<object?>`.
- Svelonia's internals map these observables directly to Avalonia properties using static metadata.
- Result: The compiler can "see" exactly which properties are being accessed, allowing it to remove unused code safely.

---

## 2. Framework Requirements

### `ISveloniaApplication`
To ensure services (like Mediator or Dialogs) work without reflection, your `App` class must implement `ISveloniaApplication`:

```csharp
public class App : Application, ISveloniaApplication
{
    public IServiceProvider? Services { get; private set; }
    // ...
}
```

### Static Mediator
Always use the AOT-safe registration for your business logic:
```csharp
// Use this
services.AddSveloniaDataAot(); 

// Instead of this (which uses reflection assembly scanning)
// services.AddSveloniaData(assembly); 
```

---

## 3. Publishing for AOT

To build a truly native, single-file executable, use the following command:

```bash
dotnet publish -c Release -r win-x64 /p:PublishAot=true /p:OptimizationPreference=Size
```

### Recommended Project Settings
Ensure your `.csproj` includes these flags to enable the AOT analyzers during development:

```xml
<PropertyGroup>
  <IsAotCompatible>true</IsAotCompatible>
  <EnableAotAnalyzer>true</EnableAotAnalyzer>
  <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
</PropertyGroup>
```

---

## 4. Trimming & Third-Party Libraries

When using third-party Avalonia libraries that are NOT AOT-compatible, you may see warnings during publish. Svelonia helps mitigate this by providing its own generated extensions, but you may still need to use a `TrimmerDescriptor` file to "keep" certain types if they are only accessed via legacy reflection paths.

> **Tip**: Svelonia's **Escape Hatch** API (`.Set(Property, value)`) is also 100% AOT safe, as it uses the property metadata objects directly.
