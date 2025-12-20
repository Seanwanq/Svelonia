using System.Collections.Concurrent;

namespace Svelonia.Data;

/// <summary>
/// 
/// </summary>
public interface IMediator
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Send(ICommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// 
/// </summary>
/// <param name="serviceProvider"></param>
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    // AOT-Safe Dispatcher Delegate
    // This must be set by the application (via generated code or manual setup)
    /// <summary>
    /// 
    /// </summary>
    public static Func<IServiceProvider, object, CancellationToken, Task<object?>>? DispatcherDelegate { get; set; }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // 1. Try AOT Dispatcher first
        if (DispatcherDelegate != null)
        {
            var result = await DispatcherDelegate(_serviceProvider, request, cancellationToken);
            return (TResponse)result!;
        }

        // 2. Fallback to Reflection (Non-AOT, legacy behaviro)
        // Only works if the handler is registered in DI generally
        return await LegacyReflectionDispatch(request, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Send(ICommand command, CancellationToken cancellationToken = default)
    {
        await Send<Unit>(command, cancellationToken);
    }

    // Legacy Reflection-based dispatch
    private static readonly ConcurrentDictionary<Type, Type> _handlerCache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<TResponse> LegacyReflectionDispatch<TResponse>(IRequest<TResponse> request, CancellationToken token)
    {
        var requestType = request.GetType();

        var handlerType = _handlerCache.GetOrAdd(requestType, t =>
        {
            var responseType = typeof(TResponse);
            var interfaceType = typeof(IRequestHandler<,>).MakeGenericType(t, responseType);
            return interfaceType;
        });

        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler registered for request type '{requestType.FullName}'. Ensure you have registered the handler in DI, or cnofigured the AOT DispatcherDelegate.");
        }

        var method = handlerType.GetMethod("Handle");
        if (method == null) throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

        var task = (Task<TResponse>)method.Invoke(handler, new object[] { request, token })!;
        return await task;
    }
}