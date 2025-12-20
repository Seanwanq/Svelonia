using Avalonia.Threading;

namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="initialValue"></param>
public class State<T>(T initialValue) : IDependency
{
    private T _value = initialValue;
    private readonly HashSet<IObserver> _observers = new();

    /// <summary>
    /// 
    /// </summary>
    public event Action<T>? OnChange;

    /// <summary>
    /// Debugging support
    /// </summary>
    public string? DebugName { get; set; }

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

    private void NotifyObservers()
    {
        if (_observers.Count == 0) return;
        foreach (var observer in new List<IObserver>(_observers))
        {
            observer.OnStateChanged();
        }
    }
}
