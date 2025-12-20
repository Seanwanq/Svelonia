using Avalonia.Controls;

namespace Svelonia.Core.Bindings;

/// <summary>
/// 
/// </summary>
public static class TextExtensions
{
    /// <summary>
    /// For TextBlock (One-way)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T BindTextContent<T>(this T control, State<string> state) where T : TextBlock
    {
        void Handler(string val) => control.Text = val;

        control.Text = state.Value;

        control.AttachedToVisualTree += (s, e) =>
        {
            control.Text = state.Value;
            state.OnChange += Handler;
        };

        control.DetachedFromVisualTree += (s, e) =>
        {
            state.OnChange -= Handler;
        };

        if (control.IsLoaded)
        {
            state.OnChange += Handler;
        }

        return control;
    }

    /// <summary>
    /// For TextBlock (One-way, generic value)
    /// </summary>
    /// <typeparam name="TControl"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="control"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static TControl BindTextContent<TControl, TValue>(this TControl control, State<TValue> state)
        where TControl : TextBlock
    {
        void Handler(TValue val) => control.Text = val?.ToString();

        control.Text = state.Value?.ToString();

        control.AttachedToVisualTree += (s, e) =>
        {
            control.Text = state.Value?.ToString();
            state.OnChange += Handler;
        };

        control.DetachedFromVisualTree += (s, e) =>
        {
            state.OnChange -= Handler;
        };

        if (control.IsLoaded)
        {
            state.OnChange += Handler;
        }

        return control;
    }

    /// <summary>
    /// For TextBox (Two-way)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T BindText<T>(this T control, State<string> state) where T : TextBox
    {
        bool isUpdating = false;

        void StateHandler(string val)
        {
            if (isUpdating) return;
            isUpdating = true;
            control.Text = val;
            isUpdating = false;
        }

        void ViewHandler(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TextBox.TextProperty)
            {
                if (isUpdating) return;
                isUpdating = true;
                state.Value = control.Text;
                isUpdating = false;
            }
        }

        control.Text = state.Value;

        control.AttachedToVisualTree += (s, e) =>
        {
            control.Text = state.Value;
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