using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Svelonia.Core.Controls;
using Svelonia.Core.Data;

namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public static partial class Svelonia
{
    private static IServiceProvider? _serviceProvider;
    private static IMediator? _mediator;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    public static void SetServiceProvider(IServiceProvider provider)
    {
        _serviceProvider = provider;
        _mediator = (IMediator?)provider.GetService(typeof(IMediator));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    public static void SetMediator(IMediator mediator)
    {
        _mediator = mediator;
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
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IMediator GetMediator()
    {
        if (_mediator != null) return _mediator;

        // Fallback: try to find it from Application.Current (if using our App setup pattern)
        if (Avalonia.Application.Current != null)
        {
            var appType = Avalonia.Application.Current.GetType();
            var servicesProp = appType.GetProperty("Services");
            if (servicesProp != null && servicesProp.GetValue(Avalonia.Application.Current) is IServiceProvider sp)
            {
                _mediator = sp.GetService(typeof(IMediator)) as IMediator;
                if (_mediator != null) return _mediator;
            }
        }

        throw new InvalidOperationException("Mediator is not configured. Call Svelonia.SetServiceProvider or ensure App.Services is exposed.");
    }

    /// <summary>
    /// Creates a reactive Resource that fetches data using the Mediator.
    /// </summary>
    public static Resource<T> Resource<T>(IRequest<T> request)
    {
        return new Resource<T>(GetMediator(), request);
    }

    /// <summary>
    /// Creates a SveloniaForm to handle command submission with validation and state.
    /// </summary>
    public static SveloniaForm<T> Form<T>()
    {
        return new SveloniaForm<T>(GetMediator());
    }

    /// <summary>
    /// Creates a SveloniaForm (void/unit) to handle command submission with validation and state.
    /// </summary>
    public static SveloniaForm Form()
    {
        return new SveloniaForm(GetMediator());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="builder"></param>
    /// <param name="animate"></param>
    /// <param name="transition"></param>
    /// <returns></returns>
    public static Control If(State<bool> condition, Func<Control> builder, bool animate = false, Animation.SveloniaTransition? transition = null)
    {
        return new IfControl(condition, builder, animate, transition);
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

}

