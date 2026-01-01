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
    .SetColor(Colors.Red)
    .OnColorChanged(e => { ... })
    .SetMargin(10);
```

## The "Escape Hatch": `.Set()`

If the Source Generator hasn't created a specific method for an Avalonia Property (e.g., a very new property or a complex generic one), you can use the universal `.Set()` extension. This ensures you never get stuck.

```csharp
using Avalonia.Controls;

new MyCustomControl()
    .Set(MyCustomControl.SomeObscureProperty, "Special Value")
    .Set(Layoutable.MarginProperty, new Thickness(10)); // Manual setter
```

---

## Native Avalonia Interop

Svelonia is 100% compatible with standard Avalonia. You can mix and match.

### 1. Using XAML-defined Controls
If you have a control defined in XAML (e.g., `MyLegacyView.axaml`), just instantiate it and use it inside your Svelonia tree.

```csharp
new StackPanel().SetChildren(
    new TextBlock().SetText("Svelonia Header"),
    new MyLegacyView(), // Standard Avalonia XAML control
    new Button().SetContent("Svelonia Footer")
);
```

### 2. Embedding Svelonia in XAML
Since a Svelonia `Component` is just a `UserControl`, you can reference it in XAML:

```xml
<Window xmlns:pages="clr-namespace:MyApp.Pages">
    <pages:MySveloniaPage />
</Window>
```

---

## Template Drilling (Advanced)

Some Avalonia controls (like `TextBox`) have complex internal structures. Svelonia's fluent API automatically attempts to find and apply styles to the correct internal parts (like `PART_ContentPresenter`).

If you need to target a specific internal part manually, use the `.OnApplyTemplate` hook or define a local `Style`.

```csharp
new Button()
    .OnHover(style => {
        // Target the internal ContentPresenter directly
        style.Setter(TemplatedControl.ForegroundProperty, Brushes.Yellow);
    });
```

---

## Performance: Large Lists (`Sve.Each`)

When rendering hundreds of items, use the `key` parameter in `Sve.Each` to enable **Efficient Reconciliation**.

```csharp
// Without Key: The entire list is rebuilt when one item changes.
// With Key: Svelonia only moves/updates the specific control for that ID.
Sve.Each(myList, 
    item => new TextBlock().SetText(item.Name), 
    item => item.Id // The Key Selector
);
```
