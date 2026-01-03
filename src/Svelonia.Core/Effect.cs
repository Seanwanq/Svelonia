using System;
using System.Collections.Generic;

namespace Svelonia.Core;

public class Effect : IObserver, IDisposable
{
    private readonly Action _action;
    private readonly HashSet<IDependency> _dependencies = new();
    private bool _isDisposed;
    private bool _isRunning;
    private bool _needsReRun;
    private readonly string _id = Guid.NewGuid().ToString()[..4];

    public Effect(Action action)
    {
        _action = action;
        Run();
    }

    public void OnStateChanged()
    {
        if (_isDisposed) return;
        
        if (_isRunning)
        {
            // LogDebug($"Effect[{_id}] marked dirty during run");
            _needsReRun = true;
            return;
        }
        
        Run();
    }

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
        if (_isDisposed) return;
        
        _isRunning = true;
        _needsReRun = false;

        try {
            // Unsubscribe to re-track
            foreach (var d in _dependencies) d.Unsubscribe(this);
            _dependencies.Clear();

            ObserverContext.Push(this);
            try
            {
                ObserverContext.ForceTrack(() => {
                    _action();
                });
            }
            finally
            {
                ObserverContext.Pop();
            }
        }
        finally {
            _isRunning = false;
            if (_needsReRun)
            {
                // LogDebug($"Effect[{_id}] re-running due to dirty bit");
                Run();
            }
        }
    }

    private void LogDebug(string msg) => Console.WriteLine($"[DEBUG] [Effect] {msg}");

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        foreach (var d in _dependencies) d.Unsubscribe(this);
        _dependencies.Clear();
    }
}
