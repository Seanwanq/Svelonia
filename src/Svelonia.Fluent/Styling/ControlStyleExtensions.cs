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
    /// Multi-state property setter helper
    /// </summary>
    public static T SetStateProperty<T>(this T control, AvaloniaProperty prop, object normal, object? hover = null, object? pressed = null, object? disabled = null) where T : Control
    {
        // For Button, we ONLY drill down for specific visual properties that are overridden by the theme template
        bool isButton = control is Button;
        bool needsDrillDown = isButton && (
            prop == TemplatedControl.BackgroundProperty ||
            prop == TemplatedControl.ForegroundProperty ||
            prop == TemplatedControl.BorderBrushProperty ||
            prop == TemplatedControl.BorderThicknessProperty ||
            prop == TemplatedControl.CornerRadiusProperty ||
            prop == TemplatedControl.PaddingProperty
        );

        Func<Selector?, Selector> target = needsDrillDown
            ? x => x.OfType<T>().Template().OfType<ContentPresenter>()
            : x => x.OfType<T>();

        // Normal
        control.AddFluentStyle(
            x => target(x),
            s => s.Setters.Add(new Setter(prop, normal))
        );

        // Hover
        if (hover != null)
            control.AddFluentStyle(
                x => needsDrillDown ? x.OfType<T>().IsPointerOver().Template().OfType<ContentPresenter>() : x.OfType<T>().IsPointerOver(),
                s => s.Setters.Add(new Setter(prop, hover))
            );

        // Pressed
        if (pressed != null)
            control.AddFluentStyle(
                x => needsDrillDown ? x.OfType<T>().IsPressed().Template().OfType<ContentPresenter>() : x.OfType<T>().IsPressed(),
                s => s.Setters.Add(new Setter(prop, pressed))
            );

        // Disabled
        if (disabled != null)
            control.AddFluentStyle(
                x => needsDrillDown ? x.OfType<T>().IsDisabled().Template().OfType<ContentPresenter>() : x.OfType<T>().IsDisabled(),
                s => s.Setters.Add(new Setter(prop, disabled))
            );

        return control;
    }

    /// <summary>
    /// Multi-state Background
    /// </summary>
    public static T Background<T>(this T control, object normal, object? hover = null, object? pressed = null, object? disabled = null) where T : Control
    {
        var prop = control is Panel ? Panel.BackgroundProperty : 
                   control is Border ? Border.BackgroundProperty :
                   control is TemplatedControl ? TemplatedControl.BackgroundProperty :
                   control is ContentPresenter ? ContentPresenter.BackgroundProperty :
                   null;

        if (prop == null) return control;
        return control.SetStateProperty(prop, normal, hover, pressed, disabled);
    }

    /// <summary>
    /// Multi-state Foreground
    /// </summary>
    public static T Foreground<T>(this T control, object normal, object? hover = null, object? pressed = null, object? disabled = null) where T : Control
    {
        var prop = control is TemplatedControl ? TemplatedControl.ForegroundProperty :
                   control is TextBlock ? TextBlock.ForegroundProperty :
                   control is ContentPresenter ? ContentPresenter.ForegroundProperty :
                   null;

        if (prop == null) return control;
        return control.SetStateProperty(prop, normal, hover, pressed, disabled);
    }

    /// <summary>
    /// Multi-state Padding
    /// </summary>
    public static T Padding<T>(this T control, object normal, object? hover = null, object? pressed = null, object? disabled = null) where T : Control
    {
        var prop = control is TemplatedControl ? TemplatedControl.PaddingProperty :
                   control is Decorator ? Decorator.PaddingProperty :
                   control is TextBlock ? TextBlock.PaddingProperty :
                   null;

        if (prop == null) return control;
        return control.SetStateProperty(prop, normal, hover, pressed, disabled);
    }

    /// <summary>
    /// Multi-state BorderThickness
    /// </summary>
    public static T BorderThickness<T>(this T control, object normal, object? hover = null, object? pressed = null, object? disabled = null) where T : Control
    {
        var prop = control is TemplatedControl ? TemplatedControl.BorderThicknessProperty :
                   control is Border ? Border.BorderThicknessProperty :
                   null;

        if (prop == null) return control;
        return control.SetStateProperty(prop, normal, hover, pressed, disabled);
    }

    /// <summary>
    /// Multi-state CornerRadius
    /// </summary>
    public static T CornerRadius<T>(this T control, object normal, object? hover = null, object? pressed = null, object? disabled = null) where T : Control
    {
        var prop = control is TemplatedControl ? TemplatedControl.CornerRadiusProperty :
                   control is Border ? Border.CornerRadiusProperty :
                   null;

        if (prop == null) return control;
        return control.SetStateProperty(prop, normal, hover, pressed, disabled);
    }

    public static T OnHover<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x.IsPointerOver(), configure);

    public static T OnPressed<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x.IsPressed(), configure);

    public static T OnDisabled<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x.IsDisabled(), configure);
}
