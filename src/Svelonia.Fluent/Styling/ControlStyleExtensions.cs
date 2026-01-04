using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Input;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class ControlStyleExtensions
{
    private static AvaloniaProperty? GetBackgroundProperty(Control control)
    {
        return control switch
        {
            TemplatedControl => TemplatedControl.BackgroundProperty,
            Panel => Panel.BackgroundProperty,
            Border => Border.BackgroundProperty,
            ContentPresenter => ContentPresenter.BackgroundProperty,
            _ => null
        };
    }

    private static AvaloniaProperty? GetForegroundProperty(Control control)
    {
        return control switch
        {
            TemplatedControl => TemplatedControl.ForegroundProperty,
            TextBlock => TextBlock.ForegroundProperty,
            ContentPresenter => ContentPresenter.ForegroundProperty,
            _ => null
        };
    }

    public static T Bg<T>(this T control, IBrush? brush) where T : Control
    {
        var prop = GetBackgroundProperty(control);
        if (prop != null) control.SetValue(prop, brush);
        return control;
    }

    public static T Bg<T>(this T control, State<IBrush> brush) where T : Control
    {
        var prop = GetBackgroundProperty(control);
        if (prop != null) control.Bind(prop, brush.ToBinding());
        return control;
    }

    public static T Fg<T>(this T control, IBrush? brush) where T : Control
    {
        var prop = GetForegroundProperty(control);
        if (prop != null) control.SetValue(prop, brush);
        return control;
    }

    public static T Fg<T>(this T control, State<IBrush> brush) where T : Control
    {
        var prop = GetForegroundProperty(control);
        if (prop != null) control.Bind(prop, brush.ToBinding());
        return control;
    }

    public static T Opacity<T>(this T control, double value) where T : Control
    {
        control.Opacity = value;
        return control;
    }

    public static T SetStateProperty<T, V>(this T control, AvaloniaProperty<V> property, object? normal, object? hover = null, object? pressed = null, object? disabled = null, object? focus = null) where T : Control
    {
        control.ApplySingleState(property, normal);
        
        if (hover != null) control.WhenHovered(s => s.Setter(property, SveConverter.Convert(property, hover)!));
        if (pressed != null) control.WhenPressed(s => s.Setter(property, SveConverter.Convert(property, pressed)!));
        if (disabled != null) control.WhenDisabled(s => s.Setter(property, SveConverter.Convert(property, disabled)!));
        
        if (focus != null)
        {
            var style = new Style(s => s.OfType<T>().PropertyEquals(Control.IsFocusedProperty, true));
            style.Setters.Add(new Setter(property, SveConverter.Convert(property, focus)));
            control.Styles.Add(style);
        }

        return control;
    }

    internal static void ApplySingleState<T, V>(this T control, AvaloniaProperty<V> property, object? value) where T : Control
    {
        if (value == null) return;
        
        if (value is State<V> state)
        {
            control.Bind(property, state.ToBinding());
        }
        else if (value is IState ista)
        {
            // Handle State<UnknownType> by using the generic object binding
            control.Bind(property, ista.ToBinding());
        }
        else if (value is Avalonia.Data.IBinding binding)
        {
            // Direct support for any Avalonia binding
            control.Bind(property, binding);
        }
        else if (value is Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension dr)
        {
            // Correct way to apply a DynamicResource programmatically in Avalonia 11
            if (dr.ProvideValue(null!) is Avalonia.Data.IBinding b)
            {
                control.Bind(property, b);
            }
        }
        else
        {
            control.SetValue(property, SveConverter.Convert(property, value));
        }
    }

    private static T AddFluentStyle<T>(this T control, Func<Selector?, Selector?> selectorFunc, Action<Style> configure) where T : Control
    {
        var style = new Style(s => selectorFunc(s.OfType<T>()));
        configure(style);
        control.Styles.Add(style);
        return control;
    }

    public static T WhenHovered<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x!.PropertyEquals(Control.IsPointerOverProperty, true), configure);

    public static T WhenPressed<T>(this T control, Action<Style> configure) where T : Control
    {
        // 模拟按下状态：通过添加/移除 "pressed" 样式类
        control.PointerPressed += (s, e) => 
        {
            if (e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
                control.Classes.Add("pressed");
        };
        control.PointerReleased += (s, e) => control.Classes.Remove("pressed");
        control.PointerCaptureLost += (s, e) => control.Classes.Remove("pressed");

        // 统一监听 ".pressed" 类
        return control.AddFluentStyle(x => x!.Class("pressed"), configure);
    }

    public static T WhenDisabled<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x!.PropertyEquals(Control.IsEnabledProperty, false), configure);
}