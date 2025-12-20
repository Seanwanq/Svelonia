namespace Svelonia.Data;

/// <summary>
/// Marker interface for a request that returns a response.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IRequest<TResponse> { }

/// <summary>
/// Marker interface for a request that does not return a value (void).
/// </summary>
public interface ICommand : IRequest<Unit> { }

/// <summary>
/// Represeents a void type, since C# System.Void is not valid in generics.
/// </summary>
public struct Unit
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly Unit Value = new Unit();
}

/// <summary>
/// Defines a handler for a request
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}