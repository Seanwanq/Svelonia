using System;
using System.Collections.Generic;

namespace Svelonia.Core;

/// <summary>
/// A reactive effect that runs immediately and re-runs whenever its dependencies change.
/// Used for side effects (e.g. logging, manual DOM manipulation, focus management).
/// </summary>
public class Effect : IObserver, IDisposable
{
    private readonly Action _action;
    private readonly HashSet<IDependency> _dependencies = new();
    private bool _isDisposed;

    /// <summary>
    /// Creates and starts a new Effect.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public Effect(Action action)
    {
        _action = action;
        Run();
    }

    /// <inheritdoc />
    public void OnStateChanged()
    {
        if (_isDisposed) return;
        Run();
    }

    /// <inheritdoc />
    public void RegisterDependency(IDependency dependency)
    {
        if (_isDisposed) return;
        if (_dependencies.Add(dependency))
        {
            dependency.Subscribe(this);
        }
    }

    private void Run()
    {
        // Unsubscribe from previous dependencies to handle dynamic branching
        foreach (var d in _dependencies) d.Unsubscribe(this);
        _dependencies.Clear();

        ObserverContext.Push(this);
        try
        {
            _action();
        }
        finally
        {
            ObserverContext.Pop();
        }
    }

    /// <summary>
    /// Stops the effect and unsubscribes from all dependencies.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        foreach (var d in _dependencies) d.Unsubscribe(this);
        _dependencies.Clear();
    }
}
