using Avalonia.Threading;

namespace Svelonia.Core;

/// <summary>
/// Base interface for all state objects
/// </summary>
public interface IState : IDependency
{
    /// <summary>
    /// Gets the current value as an object
    /// </summary>
    object? ValueObject { get; }
    
    /// <summary>
    /// Triggered when the value changes
    /// </summary>
    event Action<object?>? OnChangeObject;
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="initialValue"></param>
public class State<T>(T initialValue) : IState
{
    private T _value = initialValue;
    private readonly HashSet<IObserver> _observers = new();

    /// <summary>
    /// 
/// </summary>
    public event Action<T>? OnChange;
    
    /// <inheritdoc />
    public event Action<object?>? OnChangeObject;

    /// <summary>
    /// Debugging support
    /// </summary>
    public string? DebugName { get; set; }

    /// <inheritdoc />
    public object? ValueObject => Value;

    /// <summary>
    /// 
/// </summary>
    public T Value
    {
        get
        {
            ObserverContext.Current?.RegisterDependency(this);
            return _value;
        }
        set
        {
            if (Equals(_value, value)) return;

            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() => Value = value);
                return;
            }

            _value = value;
            OnChange?.Invoke(_value);
            OnChangeObject?.Invoke(_value);
            NotifyObservers();

            StateDebug.NotifyChange(this, _value);
        }
    }

    /// <summary>
    /// 
/// </summary>
    /// <param name="observer"></param>
    public void Subscribe(IObserver observer)
    {
        _observers.Add(observer);
    }

    /// <summary>
    /// 
/// </summary>
    /// <param name="observer"></param>
    public void Unsubscribe(IObserver observer)
    {
        _observers.Remove(observer);
    }

    /// <summary>
    /// Manually trigger the OnChange event and notify observers.
    /// </summary>
    public void Notify()
    {
        OnChange?.Invoke(_value);
        NotifyObservers();
    }

    private void NotifyObservers()
    {
        if (_observers.Count == 0) return;
        foreach (var observer in new List<IObserver>(_observers))
        {
            observer.OnStateChanged();
        }
    }
}
