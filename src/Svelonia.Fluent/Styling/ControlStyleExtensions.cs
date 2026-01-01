using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Data;
using System.Reactive.Linq;
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
    public static T SetStateProperty<T>(this T control, AvaloniaProperty prop, object? normal, object? hover = null, object? pressed = null, object? disabled = null, object? focus = null) where T : Control
    {
        // 1. Unwrap and Convert
        normal = SveConverter.Convert(prop, Unwrap(normal));
        hover = SveConverter.Convert(prop, Unwrap(hover));
        pressed = SveConverter.Convert(prop, Unwrap(pressed));
        disabled = SveConverter.Convert(prop, Unwrap(disabled));
        focus = SveConverter.Convert(prop, Unwrap(focus));

        // 2. Lightweight Styling Strategy for TextBox (Fluent Theme)
        if (control is TextBox)
        {
            if (TrySetTextBoxResource(control, prop, normal, hover, pressed, disabled, focus))
            {
                return control;
            }
        }

        // 3. Standard Styling Strategy
        bool isButton = control is Button;
        bool isCheckBox = control is CheckBox;
        
        bool needsDrillDown = (isButton || isCheckBox) && (
            prop == TemplatedControl.BackgroundProperty ||
            prop == TemplatedControl.ForegroundProperty ||
            prop == TemplatedControl.BorderBrushProperty ||
            prop == TemplatedControl.BorderThicknessProperty ||
            prop == TemplatedControl.CornerRadiusProperty ||
            prop == TemplatedControl.PaddingProperty
        );

        Func<Selector?, Selector> target = needsDrillDown
            ? (isButton 
                ? x => x.OfType<T>().Template().OfType<ContentPresenter>()
                : x => x.OfType<T>().Template().Name("NormalRectangle")) // CheckBox
            : x => x.OfType<T>();

        // Normal
        if (normal != null)
        {
            control.AddFluentStyle(
                x => target(x),
                s => TryAddSetter(control, s, prop, normal)
            );
        }

        // States
        if (hover != null) control.AddFluentStyle(x => needsDrillDown ? (isButton ? x.OfType<T>().IsPointerOver().Template().OfType<ContentPresenter>() : x.OfType<T>().IsPointerOver().Template().Name("NormalRectangle")) : x.OfType<T>().IsPointerOver(), s => TryAddSetter(control, s, prop, hover));
        if (pressed != null) control.AddFluentStyle(x => needsDrillDown ? (isButton ? x.OfType<T>().IsPressed().Template().OfType<ContentPresenter>() : x.OfType<T>().IsPressed().Template().Name("NormalRectangle")) : x.OfType<T>().IsPressed(), s => TryAddSetter(control, s, prop, pressed));
        if (focus != null) control.AddFluentStyle(x => needsDrillDown ? (isButton ? x.OfType<T>().IsFocused().Template().OfType<ContentPresenter>() : x.OfType<T>().IsFocused().Template().Name("NormalRectangle")) : x.OfType<T>().IsFocused(), s => TryAddSetter(control, s, prop, focus));
        if (disabled != null) control.AddFluentStyle(x => needsDrillDown ? (isButton ? x.OfType<T>().IsDisabled().Template().OfType<ContentPresenter>() : x.OfType<T>().IsDisabled().Template().Name("NormalRectangle")) : x.OfType<T>().IsDisabled(), s => TryAddSetter(control, s, prop, disabled));

        return control;
    }

    private static object? Unwrap(object? val)
    {
        if (val is IState state) return state.ValueObject;
        return val;
    }

    private static void TryAddSetter(Control control, Style style, AvaloniaProperty prop, object value)
    {
        // Re-convert inside setter to be safe
        value = SveConverter.Convert(prop, value)!;

        if (value is IState state)
        {
            style.Setters.Add(new Setter(prop, new Binding { Source = state, Path = nameof(IState.ValueObject), Mode = BindingMode.OneWay }));
        }
        else if (value is Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension dr)
        {
            if (dr.ResourceKey != null)
            {
                var observable = control.GetResourceObservable(dr.ResourceKey)
                                        .Select(x => SveConverter.Convert(prop, x));
                style.Setters.Add(new Setter(prop, observable.ToBinding()));
            }
        }
        else
        {
            style.Setters.Add(new Setter(prop, value));
        }
    }

    private static bool TrySetTextBoxResource(Control control, AvaloniaProperty prop, object? normal, object? hover, object? pressed, object? disabled, object? focus)
    {
        string? baseKey = null;
        if (prop == TemplatedControl.BackgroundProperty) baseKey = "TextControlBackground";
        else if (prop == TemplatedControl.ForegroundProperty) baseKey = "TextControlForeground";
        else if (prop == TemplatedControl.BorderBrushProperty) baseKey = "TextControlBorderBrush";
        else if (prop == TemplatedControl.BorderThicknessProperty) baseKey = "TextControlBorderThemeThickness";
        else return false;

        void Set(string suffix, object? val)
        {
            if (val != null && baseKey != null)
            {
                control.Resources[baseKey + suffix] = SveConverter.Convert(prop, val);
            }
        }

        Set("", normal);
        Set("PointerOver", hover);
        Set("Pressed", pressed);
        Set("Disabled", disabled);
        Set("Focused", focus);

        return true;
    }

    public static bool ApplySingleState<T>(this T control, AvaloniaProperty prop, object? value) where T : Control
    {
        if (value == null) return false;
        
        if (value is IState state)
        {
            void Handler(object? val)
            {
                // Ensure value is converted (e.g. numeric to Thickness)
                var converted = SveConverter.Convert(prop, val);

                if (converted is Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension dr)
                {
                    control.Bind(prop, dr);
                }
                else
                {
                    control.SetValue(prop, converted);
                }
            }

            // Initial set
            Handler(state.ValueObject);

            control.AttachedToVisualTree += (s, e) =>
            {
                Handler(state.ValueObject);
                state.OnChangeObject += Handler;
            };
            control.DetachedFromVisualTree += (s, e) =>
            {
                state.OnChangeObject -= Handler;
            };
            if (control.IsLoaded) state.OnChangeObject += Handler;
            return true;
        }

        var finalValue = SveConverter.Convert(prop, value);
        if (finalValue is Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension drDirect)
        {
            if (drDirect.ResourceKey != null)
            {
                var observable = control.GetResourceObservable(drDirect.ResourceKey)
                                        .Select(x => SveConverter.Convert(prop, x));
                control.Bind(prop, observable.ToBinding());
            }
            return true;
        }

        control.SetValue(prop, finalValue!);
        return true;
    }

    /// <summary>
    /// Shorthand for BorderBrush and BorderThickness
    /// </summary>
    public static T SetBorder<T>(this T control, object brush, double thickness = 1.0, object? focusBrush = null) where T : Control
    {
        var brushProp = control is Avalonia.Controls.Border ? Avalonia.Controls.Border.BorderBrushProperty :
                        control is TemplatedControl ? TemplatedControl.BorderBrushProperty : null;
        
        var thickProp = control is Avalonia.Controls.Border ? Avalonia.Controls.Border.BorderThicknessProperty :
                        control is TemplatedControl ? TemplatedControl.BorderThicknessProperty : null;

        if (brushProp != null)
            control.SetStateProperty(brushProp, brush, focus: focusBrush);
            
        if (thickProp != null)
            control.SetStateProperty(thickProp, thickness);
            
        return control;
    }

    /// <summary>
    /// Set if the control can receive focus
    /// </summary>
    public static T SetFocusable<T>(this T control, bool focusable = true) where T : Control
    {
        control.Focusable = focusable;
        return control;
    }

    public static T OnHover<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x!.IsPointerOver(), configure);

    public static T OnPressed<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x!.IsPressed(), configure);

    public static T OnDisabled<T>(this T control, Action<Style> configure) where T : Control
        => control.AddFluentStyle(x => x!.IsDisabled(), configure);
}
