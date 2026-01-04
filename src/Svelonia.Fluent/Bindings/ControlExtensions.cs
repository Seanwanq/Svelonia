using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class ControlExtensions
{
    /// <summary>
    /// Auto-focuses the control when it is attached to the visual tree.
    /// </summary>
    public static T AutoFocus<T>(this T control)
        where T : Control
    {
        control.AttachedToVisualTree += (s, e) => control.Focus();
        return control;
    }

    /// <summary>
    /// Binds the focus state of the control to a State of boolean.
    /// When the state becomes true, the control is focused.
    /// </summary>
    public static T BindFocus<T>(this T control, State<bool> focusState)
        where T : Control
    {
        Action<bool> update = focused =>
        {
            if (focused)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(
                    () =>
                    {
                        if (control.IsVisible)
                            control.Focus();
                    },
                    Avalonia.Threading.DispatcherPriority.Input
                );
            }
        };

        focusState.OnChange += update;
        control.RegisterDisposable(new AnonymousDisposable(() => focusState.OnChange -= update));

        if (focusState.Value)
            update(true);

        return control;
    }

    private class AnonymousDisposable(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }

    public static T BindVisible<T>(this T control, State<bool> state)
        where T : Control
    {
        control.Bind(Visual.IsVisibleProperty, state.ToBinding());
        return control;
    }

    public static T BindBorderBrush<T>(this T control, State<IBrush> state)
        where T : Control
    {
        var prop =
            control is Border ? Border.BorderBrushProperty
            : control is TemplatedControl ? TemplatedControl.BorderBrushProperty
            : null;
        if (prop != null)
            control.Bind(prop, state.ToBinding());
        return control;
    }

    public static T BindBorderThickness<T>(this T control, State<Thickness> state)
        where T : Control
    {
        var prop =
            control is Border ? Border.BorderThicknessProperty
            : control is TemplatedControl ? TemplatedControl.BorderThicknessProperty
            : null;
        if (prop != null)
            control.Bind(prop, state.ToBinding());
        return control;
    }

    public static T SetBorderBrush<T>(this T control, IBrush? value)
        where T : Control
    {
        if (control is Border b)
            b.BorderBrush = value;
        else if (control is TemplatedControl tc)
            tc.BorderBrush = value;
        return control;
    }

    public static T SetBorderThickness<T>(this T control, Thickness value)
        where T : Control
    {
        if (control is Border b)
            b.BorderThickness = value;
        else if (control is TemplatedControl tc)
            tc.BorderThickness = value;
        return control;
    }

    /// <summary>
    /// Sets both BorderBrush and BorderThickness.
    /// </summary>
    public static T SetBorder<T>(this T control, IBrush? brush, double thickness = 1.0)
        where T : Control
    {
        control.SetBorderBrush(brush);
        control.SetBorderThickness(new Thickness(thickness));
        return control;
    }

    public static T BindTwoWay<T, V>(this T control, AvaloniaProperty<V> property, State<V> state)
        where T : Control
    {
        var binding = new Binding("Value") { Source = state, Mode = BindingMode.TwoWay };
        control.Bind(property, binding);
        return control;
    }

    /// <summary>
    /// Binds the Text property of a TextBlock to a State.
    /// </summary>
    public static TextBlock BindText(this TextBlock control, State<string> state)
    {
        control.Bind(TextBlock.TextProperty, state.ToBinding());
        return control;
    }

    /// <summary>
    /// Binds the Text property of a TextBox to a State with TwoWay mode.
    /// </summary>
    public static TextBox BindText(this TextBox control, State<string> state)
    {
        return control.BindTwoWay(TextBox.TextProperty, state);
    }

    /// <summary>
    /// Binds a TextBox to a BufferedState, automatically handling Enter (commit) and Escape (reset).
    /// </summary>
    public static TextBox BindBufferedText(
        this TextBox control,
        BufferedState<string> buffer,
        State<bool>? isEditing = null
    )
    {
        control.BindText(buffer);

        control.OnKey(
            "Enter",
            () =>
            {
                buffer.Commit();
                if (isEditing != null)
                    isEditing.Value = false;
            },
            handled: true
        );

        control.OnKey(
            "Escape",
            () =>
            {
                buffer.Reset();
                if (isEditing != null)
                    isEditing.Value = false;
            },
            handled: true
        );

        // Optional: Sync buffer when editing starts
        if (isEditing != null)
        {
            isEditing.OnChange += editing =>
            {
                if (editing)
                    buffer.Reset();
            };
        }

        return control;
    }

    public static T BindIsChecked<T>(this T control, State<bool?> state)
        where T : ToggleButton
    {
        return control.BindTwoWay(ToggleButton.IsCheckedProperty, state);
    }

    public static T BindValue<T>(this T control, State<double> state)
        where T : RangeBase
    {
        return control.BindTwoWay(RangeBase.ValueProperty, state);
    }

    public static TextBox SetText(this TextBox control, string? value)
    {
        control.Text = value;
        return control;
    }

    public static T SetIsChecked<T>(this T control, bool? value)
        where T : ToggleButton
    {
        control.IsChecked = value;
        return control;
    }

    public static T SetValue<T>(this T control, double value)
        where T : RangeBase
    {
        control.Value = value;
        return control;
    }
}
