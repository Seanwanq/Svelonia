namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public interface IDependency
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="observer"></param>
    void Subscribe(IObserver observer);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="observer"></param>
    void Unsubscribe(IObserver observer);
}

/// <summary>
/// 
/// </summary>
public interface IObserver
{
    /// <summary>
    /// 
    /// </summary>
    void OnStateChanged();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dependency"></param>
    void RegisterDependency(IDependency dependency);
}

internal static class ObserverContext
{
    [ThreadStatic]
    private static Stack<IObserver>? _stack;

    public static void Push(IObserver observer)
    {
        _stack ??= new Stack<IObserver>();
        _stack.Push(observer);
    }

    public static void Pop()
    {
        _stack?.Pop();
    }

    public static IObserver? Current => (_stack != null && _stack.Count > 0) ? _stack.Peek() : null;
}