namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class Computed<T> : State<T>, IObserver, IDisposable
{
    private readonly Func<T> _computer;
    private readonly HashSet<IDependency> _dependencies = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="computer"></param>
    public Computed(Func<T> computer) : base(default!)
    {
        _computer = computer;
        Recompute();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnStateChanged()
    {
        Recompute();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dependency"></param>
    public void RegisterDependency(IDependency dependency)
    {
        if (_dependencies.Add(dependency))
        {
            dependency.Subscribe(this);
        }
    }

    private void Recompute()
    {
        // Unsubscribe from previous dependencies to handle dynamic branching
        foreach (var d in _dependencies) d.Unsubscribe(this);
        _dependencies.Clear();

        ObserverContext.Push(this);
        try
        {
            var newValue = _computer();
            // Update value. If changed, State<T> will notify downstream observers.
            Value = newValue;
        }
        finally
        {
            ObserverContext.Pop();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        foreach (var d in _dependencies) d.Unsubscribe(this);
        _dependencies.Clear();
    }
}