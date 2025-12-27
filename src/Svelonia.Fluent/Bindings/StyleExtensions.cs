using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
public static class StyleExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T Background<T>(this T control, State<IBrush> state) where T : Control
    {
        AvaloniaProperty? prop = null;
        if (control is Border) prop = Border.BackgroundProperty;
        else if (control is Panel) prop = Panel.BackgroundProperty;
        else if (control is TemplatedControl) prop = TemplatedControl.BackgroundProperty;

        if (prop != null)
        {
            Bind(control, prop, state);
        }

        return control;
    }

    private static void Bind<TControl, TValue>(TControl control, AvaloniaProperty prop, State<TValue> state)
        where TControl : AvaloniaObject
    {
        void Handler(TValue val)
        {
            if (val is Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension dr)
                control.Bind(prop, dr);
            else
                control.SetValue(prop, val);
        }

        Handler(state.Value);

        if (control is Control c)
        {
            c.AttachedToVisualTree += (s, e) =>
            {
                Handler(state.Value);
                state.OnChange += Handler;
            };
            c.DetachedFromVisualTree += (s, e) =>
            {
                state.OnChange -= Handler;
            };
            if (c.IsLoaded) state.OnChange += Handler;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    public static T BindContent<T>(this T control, State<string> state) where T : ContentControl
    {
        void Handler(string val) => control.Content = val;

        control.Content = state.Value;

        if (control is Control c)
        {
            c.AttachedToVisualTree += (s, e) =>
            {
                control.Content = state.Value;
                state.OnChange += Handler;
            };
            c.DetachedFromVisualTree += (s, e) =>
            {
                state.OnChange -= Handler;
            };
            if (c.IsLoaded) state.OnChange += Handler;
        }
        return control;
    }
}
