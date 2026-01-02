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
}
