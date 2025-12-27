using Avalonia;
using Avalonia.Controls;

using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
public static class Binder
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TControl"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="control"></param>
    /// <param name="prop"></param>
    /// <param name="state"></param>
    public static void Bind<TControl, TValue>(TControl control, AvaloniaProperty prop, State<TValue> state)
        where TControl : AvaloniaObject
    {
        void Handler(TValue val)
        {
            if (val is Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension dr)
            {
                control.Bind(prop, dr);
            }
            else
            {
                control.SetValue(prop, val);
            }
        }

        // Initial set
        Handler(state.Value);

        // Lifecycle management
        if (control is Control c)
        {
            // Subscribe on attach, Unsubscribe on detach
            c.AttachedToVisualTree += (s, e) =>
            {
                // Sync again in case it changed while detached
                control.SetValue(prop, state.Value);
                state.OnChange += Handler;
            };
            c.DetachedFromVisualTree += (s, e) =>
            {
                state.OnChange -= Handler;
            };

            // If already loaded/attached, subscribe immediately
            if (c.IsLoaded)
            {
                state.OnChange += Handler;
            }
        }
    }
}