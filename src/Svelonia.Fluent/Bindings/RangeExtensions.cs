using Avalonia.Controls.Primitives;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class RangeExtensions
{
    public static T BindValue<T>(this T control, State<double> state)
        where T : RangeBase
    {
        bool isUpdating = false;

        void StateHandler(double val)
        {
            if (isUpdating)
                return;
            isUpdating = true;
            control.Value = val;
            isUpdating = false;
        }

        void ViewHandler(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == RangeBase.ValueProperty)
            {
                if (isUpdating)
                    return;
                isUpdating = true;
                state.Value = control.Value;
                isUpdating = false;
            }
        }

        control.Value = state.Value;

        control.AttachedToVisualTree += (s, e) =>
        {
            control.Value = state.Value;
            state.OnChange += StateHandler;
            control.PropertyChanged += ViewHandler;
        };

        control.DetachedFromVisualTree += (s, e) =>
        {
            state.OnChange -= StateHandler;
            control.PropertyChanged -= ViewHandler;
        };

        if (control.IsLoaded)
        {
            state.OnChange += StateHandler;
            control.PropertyChanged += ViewHandler;
        }

        return control;
    }
}
