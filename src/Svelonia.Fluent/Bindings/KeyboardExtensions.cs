using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Svelonia.Core;
using Svelonia.Data;
namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
public static class KeyboardExtensions
{
    /// <summary>
    /// Binds a key gesture to an action.
    /// </summary>
    /// <param name="control">The control to attach the listener to.</param>
    /// <param name="gesture">The key gesture string (e.g., "Ctrl+S", "Enter").</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="handled">If true, marks the event as handled (prevent default). Default is true.</param>
    /// <param name="tunnel">If true, uses PreviewKeyDown (Tunneling) instead of KeyDown (Bubbling).</param>
    public static T OnKey<T>(this T control, string gesture, Action action, bool handled = true, bool tunnel = false)
        where T : Control
    {
        var keyGesture = KeyGesture.Parse(gesture);

        void Handler(object? sender, KeyEventArgs e)
        {
            if (keyGesture.Matches(e))
            {
                action();
                if (handled) e.Handled = true;
            }
        }

        if (tunnel)
            control.AddHandler(InputElement.KeyDownEvent, Handler, RoutingStrategies.Tunnel);
        else
            control.KeyDown += Handler;

        return control;
    }

    /// <summary>
    /// Binds a key gesture to a Mediator Command.
    /// </summary>
    public static T OnKey<T>(this T control, string gesture, ICommand command, bool handled = true, bool tunnel = false)
        where T : Control
    {
        return control.OnKey(gesture, () =>
        {
            // Resolve mediator dynamically
            if (Avalonia.Application.Current is Application app &&
                app.GetType().GetProperty("Services")?.GetValue(app) is IServiceProvider sp &&
                sp.GetService(typeof(IMediator)) is IMediator mediator)
            {
                mediator.Send(command);
            }
        }, handled, tunnel);
    }

    /// <summary>
    /// Binds a key gesture to a Mediator Command Factory.
    /// </summary>
    public static T OnKey<T>(this T control, string gesture, Func<ICommand> commandFactory, bool handled = true, bool tunnel = false)
        where T : Control
    {
        return control.OnKey(gesture, () =>
        {
            if (Avalonia.Application.Current is Application app &&
                app.GetType().GetProperty("Services")?.GetValue(app) is IServiceProvider sp &&
                sp.GetService(typeof(IMediator)) is IMediator mediator)
            {
                mediator.Send(commandFactory());
            }
        }, handled, tunnel);
    }
}
