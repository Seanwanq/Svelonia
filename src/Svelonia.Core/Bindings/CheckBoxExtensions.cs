using Avalonia.Controls;

namespace Svelonia.Core.Bindings;

/// <summary>
/// 
/// </summary>
public static class CheckBoxExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T IsChecked<T>(this T control, State<bool> state) where T : CheckBox
    {
        bool isUpdating = false;

        void StateHandler(bool val)
        {
            if (isUpdating) return;
            isUpdating = true;
            control.IsChecked = val;
            isUpdating = false;
        }

        void ViewHandler(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == CheckBox.IsCheckedProperty)
            {
                if (isUpdating) return;
                isUpdating = true;
                state.Value = control.IsChecked ?? false;
                isUpdating = false;
            }
        }

        // Initial
        control.IsChecked = state.Value;

        control.AttachedToVisualTree += (s, e) =>
        {
            control.IsChecked = state.Value;
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
