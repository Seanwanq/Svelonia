using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Svelonia.Core.Controls;

namespace Svelonia.Core;

/// <summary>
///
/// </summary>
public static partial class Svelonia { }

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
    public static Control If(
        State<bool> condition,
        Func<Control> builder,
        Func<Control>? elseBuilder = null,
        bool animate = false,
        Animation.SveloniaTransition? transition = null
    )
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
            ItemTemplate = new FuncDataTemplate<T>((item, ns) => template(item)),
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
        Func<Exception, Control>? error = null
    )
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
    /// Creates a reactive effect.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="paused">If true, the effect will not run immediately. Use <see cref="Effect.Resume"/> to start it.</param>
    /// <returns>The effect instance.</returns>
    public static Effect Effect(Action action, bool paused = false)
    {
        return new Effect(action, paused);
    }

    /// <summary>
    /// Runs the specified action without registering any reactive dependencies.
    /// </summary>
    public static void Untrack(Action action)
    {
        ObserverContext.PushUntrack();
        try
        {
            action();
        }
        finally
        {
            ObserverContext.PopUntrack();
        }
    }

    /// <summary>
    /// Runs the specified action without registering any reactive dependencies.
    /// </summary>
    public static T Untrack<T>(Func<T> func)
    {
        ObserverContext.PushUntrack();
        try
        {
            return func();
        }
        finally
        {
            ObserverContext.PopUntrack();
        }
    }

    /// <summary>
    /// Batches multiple state updates together, notifying observers only once at the end.
    /// </summary>
    public static void Batch(Action action)
    {
        ObserverContext.PushBatch();
        try
        {
            action();
        }
        finally
        {
            ObserverContext.PopBatch();
        }
    }

    /// <summary>
    /// Reactively flattens a tree structure into a flat list.
    /// </summary>
    public static Computed<IEnumerable<object>> FlattenTree<TNode>(
        TNode root,
        Func<TNode, IEnumerable<TNode>> getChildren,
        Func<TNode, bool>? isExpanded = null,
        Func<TNode, TNode, object>? projectEdge = null
    )
    {
        return TreeFlattener.Flatten(root, getChildren, isExpanded, projectEdge);
    }
}