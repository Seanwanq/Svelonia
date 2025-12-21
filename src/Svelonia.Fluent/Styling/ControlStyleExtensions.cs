using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Svelonia.Core;

namespace Svelonia.Fluent;

/// <summary>
/// Extensions for Control to apply inline styles and state-based animations.
/// </summary>
public static class ControlStyleExtensions
{
    private static T AddStyle<T>(this T control, Selector? selector, Action<Style> configure) where T : Control
    {
        var style = new Style(x => selector);
        configure(style);
        control.Styles.Add(style);
        return control;
    }

    /// <summary>
    /// Apply a style when pointer is over the control.
    /// </summary>
    public static T OnHover<T>(this T control, Action<Style> configure) where T : Control
        => control.AddStyle(((Selector?)null!).IsPointerOver(), configure);

    /// <summary>
    /// Apply a style when the control is pressed.
    /// </summary>
    public static T OnPressed<T>(this T control, Action<Style> configure) where T : Control
        => control.AddStyle(((Selector?)null!).IsPressed(), configure);

    /// <summary>
    /// Apply a style when the control is focused.
    /// </summary>
    public static T OnFocused<T>(this T control, Action<Style> configure) where T : Control
        => control.AddStyle(((Selector?)null!).IsFocused(), configure);

    /// <summary>
    /// Adds a transition animation to the control.
    /// </summary>
    public static T Transition<T>(this T control, ITransition transition) where T : Control
    {
        if (control.Transitions == null) control.Transitions = new Transitions();
        control.Transitions.Add(transition);
        return control;
    }

    /// <summary>
    /// Fluent multi-state background.
    /// </summary>
    public static T Background<T>(this T control, object normal, object? hover = null, object? pressed = null) where T : Control
    {
        // Use styles for all to ensure priority consistency
        control.AddStyle(null, s => s.Setter(TemplatedControl.BackgroundProperty, normal));
        if (hover != null) control.OnHover(s => s.Setter(TemplatedControl.BackgroundProperty, hover));
        if (pressed != null) control.OnPressed(s => s.Setter(TemplatedControl.BackgroundProperty, pressed));
        return control;
    }
}
