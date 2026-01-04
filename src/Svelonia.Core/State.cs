using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Svelonia.Core;

public interface IState : IDependency, IObservable<object?>, INotifyPropertyChanged
{
    object? ValueObject { get; }
    event Action<object?>? OnChangeObject;
}

public class State<T> : IState
{
    private static int _globalStateId = 0;
    public int StateId { get; } = ++_globalStateId;

    private T _value;
    private readonly HashSet<IObserver> _observers = new();
    private readonly BehaviorSubject<object?> _subject;

    public event PropertyChangedEventHandler? PropertyChanged;

    public State(T initialValue)
    {
        _value = initialValue;
        _subject = new BehaviorSubject<object?>(initialValue);
    }

    public event Action<T>? OnChange;
    public event Action<object?>? OnChangeObject;
    public string? DebugName { get; set; }
    public object? ValueObject => Value;

    protected T InternalValue
    {
        get => _value;
        set => _value = value;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ForceUpdate(T newValue)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => ForceUpdate(newValue));
            return;
        }

        _value = newValue;
        OnChange?.Invoke(_value);
        OnChangeObject?.Invoke(_value);
        _subject.OnNext(_value);
        
        OnPropertyChanged(nameof(Value));
        NotifyObservers();
        
        StateDebug.NotifyChange(this, _value);
    }

    /// <summary>
    /// Updates the value and notifies observers silently (skips StateDebug).
    /// Used for internal framework states to prevent recursion.
    /// </summary>
    public void SetSilent(T newValue)
    {
        _value = newValue;
        _subject.OnNext(_value);
        OnPropertyChanged(nameof(Value));
        NotifyObservers();
    }

    public virtual T Value
    {
        get
        {
            var observer = ObserverContext.Current;
            if (observer != null)
            {
                // LogDebug($"State[{StateId}]({DebugName}) being read by Observer");
                observer.RegisterDependency(this);
            }
            return _value;
        }
        set
        {
            if (Equals(_value, value)) return;
            ForceUpdate(value);
        }
    }

    public void Subscribe(IObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IObserver observer) => _observers.Remove(observer);

    public void Notify()
    {
        OnPropertyChanged(nameof(Value));
        _subject.OnNext(_value);
        NotifyObservers();
    }

    protected void NotifyObservers()
    {
        if (_observers.Count == 0) return;
        var targets = _observers.ToList();
        foreach (var observer in targets)
        {
            observer.OnStateChanged();
        }
    }

    private void LogDebug(string msg) => Console.WriteLine($"[DEBUG] [State] {msg}");

    public IDisposable Subscribe(System.IObserver<object?> observer) => _subject.Subscribe(observer);
}