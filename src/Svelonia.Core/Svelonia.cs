using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Svelonia.Core.Controls;

namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public static partial class Svelonia
{
}

/// <summary>
/// 
/// </summary>
public static partial class Sve
{
    private static IServiceProvider? _serviceProvider;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    public static void SetServiceProvider(IServiceProvider provider)
    {
        _serviceProvider = provider;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetService<T>()
    {
        return (T?)_serviceProvider?.GetService(typeof(T));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="builder"></param>
    /// <param name="elseBuilder"></param>
    /// <param name="animate"></param>
    /// <param name="transition"></param>
    /// <returns></returns>
    public static Control If(State<bool> condition, Func<Control> builder, Func<Control>? elseBuilder = null, bool animate = false, Animation.SveloniaTransition? transition = null)
    {
        return new IfControl(condition, builder, elseBuilder, animate, transition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="template"></param>
    /// <returns></returns>
    public static ItemsControl Each<T>(StateList<T> items, Func<T, Control> template)
    {
        return new ItemsControl
        {
            ItemsSource = items,
            ItemTemplate = new FuncDataTemplate<T>((item, ns) => template(item))
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="then"></param>
    /// <param name="loading"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Control Await<T>(
        Task<T> task,
        Func<T, Control> then,
        Func<Control>? loading = null,
        Func<Exception, Control>? error = null)
    {
        return new AwaitControl<T>(task, loading, then, error);
    }

    /// <summary>
    /// Creates a Dynamic Resource extension for use in fluent bindings.
    /// </summary>
    public static Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension Res(string key)
    {
        return new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension(key);
    }

    /// <summary>
    /// Creates a reactive effect that runs immediately and re-runs whenever its dependencies change.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A disposable that stops the effect.</returns>
    public static IDisposable Effect(Action action)
    {
        return new Effect(action);
    }

    /// <summary>
    /// Runs the specified action without registering any reactive dependencies.
    /// </summary>
    public static void Untrack(Action action)
    {
        ObserverContext.PushUntrack();
        try { action(); }
        finally { ObserverContext.PopUntrack(); }
    }

    /// <summary>
    /// Runs the specified function without registering any reactive dependencies.
    /// </summary>
    public static T Untrack<T>(Func<T> func)
    {
        ObserverContext.PushUntrack();
        try { return func(); }
        finally { ObserverContext.PopUntrack(); }
    }
}