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
    public static T OnKey<T>(
        this T control,
        string gesture,
        Action action,
        bool handled = true,
        bool tunnel = false
    )
        where T : Control
    {
        // Try parsing as gesture first
        KeyGesture? keyGesture = null;
        try
        {
            keyGesture = KeyGesture.Parse(gesture);
        }
        catch { }

        void Handler(object? sender, KeyEventArgs e)
        {
            bool match = false;
            if (keyGesture != null)
            {
                match = keyGesture.Matches(e);
            }
            else
            {
                // Fallback for simple key names (e.g. "A", "1") - STRICT MODE
                if (Enum.TryParse<Key>(gesture, true, out var key))
                {
                    match = e.Key == key && e.KeyModifiers == KeyModifiers.None;
                }
            }

            if (match)
            {
                action();
                if (handled)
                    e.Handled = true;
            }
        }

        if (tunnel)
            control.AddHandler(InputElement.KeyDownEvent, Handler, RoutingStrategies.Tunnel);
        else
            control.KeyDown += Handler;

        return control;
    }

    /// <summary>
    /// Binds a specific key to an action.
    /// </summary>
    public static T OnKey<T>(
        this T control,
        Key key,
        Action action,
        bool handled = true,
        bool tunnel = false
    )
        where T : Control
    {
        void Handler(object? sender, KeyEventArgs e)
        {
            if (e.Key == key && e.KeyModifiers == KeyModifiers.None)
            {
                action();
                if (handled)
                    e.Handled = true;
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
    public static T OnKey<T>(
        this T control,
        string gesture,
        ICommand command,
        bool handled = true,
        bool tunnel = false
    )
        where T : Control
    {
        return control.OnKey(
            gesture,
            () =>
            {
                // Resolve mediator via ISveloniaApplication (AOT-Safe)
                if (
                    Avalonia.Application.Current is ISveloniaApplication app
                    && app.Services?.GetService(typeof(IMediator)) is IMediator mediator
                )
                {
                    mediator.Send(command);
                }
            },
            handled,
            tunnel
        );
    }

    /// <summary>
    /// Binds a key gesture to a Mediator Command Factory.
    /// </summary>
    public static T OnKey<T>(
        this T control,
        string gesture,
        Func<ICommand> commandFactory,
        bool handled = true,
        bool tunnel = false
    )
        where T : Control
    {
        return control.OnKey(
            gesture,
            () =>
            {
                // Resolve mediator via ISveloniaApplication (AOT-Safe)
                if (
                    Avalonia.Application.Current is ISveloniaApplication app
                    && app.Services?.GetService(typeof(IMediator)) is IMediator mediator
                )
                {
                    mediator.Send(commandFactory());
                }
            },
            handled,
            tunnel
        );
    }

    /// <summary>
    /// Configures multiple keyboard shortcuts for a control using a fluent builder.
    /// Uses a single event handler for efficiency.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="config">The builder configuration.</param>
    /// <param name="when">Optional predicate to check if shortcuts should be processed.</param>
    public static T WithShortcuts<T>(this T control, Action<ShortcutBuilder> config, Func<bool>? when = null) where T : Control
    {
        var builder = new ShortcutBuilder();
        config(builder);

        control.KeyDown += (s, e) =>
        {
            if (when != null && !when()) return;

            foreach (var shortcut in builder.Shortcuts)
            {
                bool match = false;
                if (shortcut.Gesture != null)
                {
                    match = shortcut.Gesture.Matches(e);
                }
                else if (shortcut.Key.HasValue)
                {
                    // Match specific key with no modifiers by default for simple Key bindings
                    match = e.Key == shortcut.Key.Value && e.KeyModifiers == KeyModifiers.None;
                }

                if (match)
                {
                    shortcut.Action();
                    e.Handled = true;
                    break;
                }
            }
        };

        return control;
    }
}

/// <summary>
/// Builder for defining multiple keyboard shortcuts.
/// </summary>
public class ShortcutBuilder
{
    internal List<(KeyGesture? Gesture, Key? Key, Action Action)> Shortcuts { get; } = new();

    /// <summary>
    /// Adds a shortcut using a key gesture string (e.g. "Ctrl+S").
    /// </summary>
    public ShortcutBuilder Add(string gesture, Action action)
    {
        try
        {
            var g = KeyGesture.Parse(gesture);
            Shortcuts.Add((g, null, action));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Svelonia] Failed to parse key gesture '{gesture}': {ex.Message}");
        }
        return this;
    }

    /// <summary>
    /// Adds a shortcut using a specific key.
    /// </summary>
    public ShortcutBuilder Add(Key key, Action action)
    {
        Shortcuts.Add((null, key, action));
        return this;
    }

    /// <summary>
    /// Adds a shortcut using a specific key and modifiers.
    /// </summary>
    public ShortcutBuilder Add(Key key, KeyModifiers modifiers, Action action)
    {
        Shortcuts.Add((new KeyGesture(key, modifiers), null, action));
        return this;
    }
}
