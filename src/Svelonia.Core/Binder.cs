using Avalonia;
using Avalonia.Controls;

namespace Svelonia.Core;

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
    public static void Bind<TControl, TValue>(TControl control, AvaloniaProperty<TValue> prop, State<TValue> state)
        where TControl : AvaloniaObject
    {
        void Handler(TValue val) => control.SetValue(prop, val);

        // Initial set
        control.SetValue(prop, state.Value);

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