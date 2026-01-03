using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class ControlExtensions
{
    /// <summary>
    /// Auto-focuses the control when it is attached to the visual tree.
    /// </summary>
    public static T AutoFocus<T>(this T control) where T : Control
    {
        control.AttachedToVisualTree += (s, e) => control.Focus();
        return control;
    }

    /// <summary>
    /// Binds the focus state of the control to a State of boolean.
    /// When the state becomes true, the control is focused.
    /// </summary>
    public static T BindFocus<T>(this T control, State<bool> focusState) where T : Control
    {
        void Update(bool focused)
        {
            if (focused) Avalonia.Threading.Dispatcher.UIThread.Post(() => control.Focus());
        }

        focusState.OnChange += Update;
        
        // Initial state
        Update(focusState.Value);

        control.DetachedFromVisualTree += (s, e) => focusState.OnChange -= Update;

        return control;
    }

    /// <summary>
    /// Binds a property to a State with TwoWay mode.
    /// </summary>
    public static T BindTwoWay<T, V>(this T control, AvaloniaProperty<V> property, State<V> state) where T : Control
    {
        var binding = new Binding("Value")
        {
            Source = state,
            Mode = BindingMode.TwoWay
        };
        control.Bind(property, binding);
        return control;
    }

    /// <summary>
    /// Binds the Text property of a TextBox to a State with TwoWay mode.
    /// </summary>
    public static T BindText<T>(this T control, State<string> state) where T : TextBox
    {
        return control.BindTwoWay(TextBox.TextProperty, state);
    }

    /// <summary>
    /// Binds the IsChecked property of a ToggleButton (CheckBox/RadioButton) to a State with TwoWay mode.
    /// </summary>
    public static T BindIsChecked<T>(this T control, State<bool?> state) where T : Avalonia.Controls.Primitives.ToggleButton
    {
        return control.BindTwoWay(Avalonia.Controls.Primitives.ToggleButton.IsCheckedProperty, state);
    }

    /// <summary>
    /// Binds the Value property of a RangeBase control (Slider/ProgressBar) to a State with TwoWay mode.
    /// </summary>
    public static T BindValue<T>(this T control, State<double> state) where T : Avalonia.Controls.Primitives.RangeBase
    {
        return control.BindTwoWay(Avalonia.Controls.Primitives.RangeBase.ValueProperty, state);
    }

    /// <summary>
    /// Sets the Text property of a TextBox.
    /// </summary>
    public static T SetText<T>(this T control, string? value) where T : TextBox
    {
        control.Text = value;
        return control;
    }

    /// <summary>
    /// Sets the IsChecked property of a ToggleButton.
    /// </summary>
    public static T SetIsChecked<T>(this T control, bool? value) where T : Avalonia.Controls.Primitives.ToggleButton
    {
        control.IsChecked = value;
        return control;
    }

    /// <summary>
    /// Sets the Value property of a RangeBase control.
    /// </summary>
    public static T SetValue<T>(this T control, double value) where T : Avalonia.Controls.Primitives.RangeBase
    {
        control.Value = value;
        return control;
    }
}
