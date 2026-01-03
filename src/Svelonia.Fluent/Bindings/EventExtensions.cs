using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Svelonia.Fluent;

public static class EventExtensions
{
    /// <summary>
    /// Fluently attaches a handler to any routed event.
    /// </summary>
    public static T On<T, TArgs>(this T control, RoutedEvent<TArgs> routedEvent, Action<TArgs> handler, RoutingStrategies routes = RoutingStrategies.Direct | RoutingStrategies.Bubble)
        where T : Interactive
        where TArgs : RoutedEventArgs
    {
        control.AddHandler(routedEvent, (s, e) => handler(e), routes);
        return control;
    }

    /// <summary>
    /// Fluently attaches a handler to any routed event (with sender).
    /// </summary>
    public static T On<T, TArgs>(this T control, RoutedEvent<TArgs> routedEvent, Action<T, TArgs> handler, RoutingStrategies routes = RoutingStrategies.Direct | RoutingStrategies.Bubble)
        where T : Interactive
        where TArgs : RoutedEventArgs
    {
        control.AddHandler(routedEvent, (s, e) => handler((T)s!, e), routes);
        return control;
    }

    public static T OnPointerPressed<T>(this T control, Action<T, PointerPressedEventArgs> handler) where T : Interactive
        => control.On(InputElement.PointerPressedEvent, handler);

    public static T OnPointerPressed<T>(this T control, Action<PointerPressedEventArgs> handler) where T : Interactive
        => control.On(InputElement.PointerPressedEvent, handler);

    public static T OnPointerMoved<T>(this T control, Action<T, PointerEventArgs> handler) where T : Interactive
        => control.On(InputElement.PointerMovedEvent, handler);

    public static T OnPointerMoved<T>(this T control, Action<PointerEventArgs> handler) where T : Interactive
        => control.On(InputElement.PointerMovedEvent, handler);

    public static T OnPointerReleased<T>(this T control, Action<T, PointerReleasedEventArgs> handler) where T : Interactive
        => control.On(InputElement.PointerReleasedEvent, handler);

    public static T OnPointerReleased<T>(this T control, Action<PointerReleasedEventArgs> handler) where T : Interactive
        => control.On(InputElement.PointerReleasedEvent, handler);

    public static T OnKeyDown<T>(this T control, Action<T, KeyEventArgs> handler) where T : Interactive
        => control.On(InputElement.KeyDownEvent, handler);

    public static T OnKeyDown<T>(this T control, Action<KeyEventArgs> handler) where T : Interactive
        => control.On(InputElement.KeyDownEvent, handler);

    public static T OnKeyUp<T>(this T control, Action<KeyEventArgs> handler) where T : Interactive
        => control.On(InputElement.KeyUpEvent, handler);

    public static T OnTextInput<T>(this T control, Action<TextInputEventArgs> handler) where T : Interactive
        => control.On(InputElement.TextInputEvent, handler);

    public static T OnLostFocus<T>(this T control, Action<RoutedEventArgs> handler) where T : Interactive
        => control.On(InputElement.LostFocusEvent, handler);

    public static T OnGotFocus<T>(this T control, Action<RoutedEventArgs> handler) where T : Interactive
        => control.On(InputElement.GotFocusEvent, handler);
        
    public static T OnLoaded<T>(this T control, Action<T, RoutedEventArgs> handler) where T : Control
        => control.On(Control.LoadedEvent, handler);

    public static T OnLoaded<T>(this T control, Action<RoutedEventArgs> handler) where T : Control
        => control.On(Control.LoadedEvent, handler);
}
