using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class ControlStyleExtensions
{
    private static T AddFluentStyle<T>(this T control, Func<Selector?, Selector> selectorFunc, Action<Style> configure) where T : Control
    {
        var style = new Style(x => selectorFunc(x.OfType(control.GetType())));
        configure(style);
        control.Styles.Add(style);
        return control;
    }

    /// <summary>
    /// Added CSS class (to activate globally predefined animations)
    /// </summary>
    public static T WithClass<T>(this T control, string className) where T : Control
    {
        control.Classes.Add(className);
        return control;
    }

    /// <summary>
    /// Enable standard transition animations (Background, Opacity, CornerRadius)
    /// </summary>
    public static T Animate<T>(this T control) where T : Control
        => control.WithClass("trans-all");

    /// <summary>
    /// Multi-state Background (with template penetration optimization for Button and other templated controls)
    /// </summary>
    public static T Background<T>(this T control, object normal, object? hover = null, object? pressed = null) where T : Control
    {
        var prop = control is Panel ? Panel.BackgroundProperty : TemplatedControl.BackgroundProperty;

        // If it's a Button, we need to set the style for the ContentPresenter inside the template, otherwise it will be overridden by FluentTheme
        Func<Selector?, Selector> target = control is Button
            ? x => x.OfType<T>().Template().OfType<ContentPresenter>()
            : x => x.OfType<T>();

        // Normal
        control.AddFluentStyle(x => target(x), s => s.Setters.Add(new Setter(prop, normal)));

        // Hover
        if (hover != null)
            control.AddFluentStyle(x => target(x).IsPointerOver(), s => s.Setters.Add(new Setter(prop, hover)));

        // Pressed
        if (pressed != null)
            control.AddFluentStyle(x => target(x).IsPressed(), s => s.Setters.Add(new Setter(prop, pressed)));

        return control;
    }

    public static T OnHover<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x.IsPointerOver(), configure);

    public static T OnPressed<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x.IsPressed(), configure);
}
