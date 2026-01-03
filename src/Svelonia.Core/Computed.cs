using System;
using System.Collections.Generic;
using System.Linq;

namespace Svelonia.Core;

public class Computed<T> : State<T>, IObserver, IDisposable
{
    private readonly Func<T> _computer;
    private readonly HashSet<IDependency> _dependencies = new();
    private bool _isDisposed;
    private bool _isComputing;
    private bool _needsRecompute;

    public Computed(Func<T> computer) : base(default!)
    {
        _computer = computer;
        Recompute();
    }

    public void OnStateChanged()
    {
        if (_isDisposed) return;
        
        if (_isComputing)
        {
            _needsRecompute = true;
            return;
        }
        
        Recompute();
    }

    public void RegisterDependency(IDependency dependency)
    {
        if (_isDisposed) return;
        if (_dependencies.Add(dependency))
        {
            dependency.Subscribe(this);
        }
    }

    private void Recompute()
    {
        if (_isDisposed) return;
        
        _isComputing = true;
        _needsRecompute = false;

        try
        {
            foreach (var d in _dependencies) d.Unsubscribe(this);
            _dependencies.Clear();

            ObserverContext.Push(this);
            try
            {
                T? newValue = default;
                ObserverContext.ForceTrack(() => {
                    newValue = _computer();
                });
                Value = newValue!;
            }
            finally
            {
                ObserverContext.Pop();
            }
        }
        finally
        {
            _isComputing = false;
            
            if (_needsRecompute)
            {
                Recompute();
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        foreach (var d in _dependencies) d.Unsubscribe(this);
        _dependencies.Clear();
    }
}