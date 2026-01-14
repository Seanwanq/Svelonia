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
    private bool _isPaused;
    private readonly string _id = Guid.NewGuid().ToString()[..4];

    public Effect(Action action, bool paused = false)
    {
        _action = action;
        _isPaused = paused;
        
        if (!paused)
        {
            Run();
        }
    }

    public void Resume()
    {
        if (_isDisposed) return;
        if (_isPaused)
        {
            _isPaused = false;
            Run();
        }
    }

    public void OnStateChanged()
    {
        if (_isDisposed || _isPaused) return;
        
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
        if (_isDisposed || _isPaused) return;
        
        _isRunning = true;
        _needsReRun = false;

        try {
            // Unsubscribe to re-track
            foreach (var d in _dependencies) d.Unsubscribe(this);
            int oldDepCount = _dependencies.Count;
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
