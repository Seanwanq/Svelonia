using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Threading;
using Svelonia.Core;

namespace Svelonia.Physics;

/// <summary>
/// A global animation loop that batches all active spring updates into a single UI frame.
/// </summary>
public static class AnimationLoop
{
    private static readonly HashSet<SpringState> _activeSprings = new();
    private static bool _isRunning;
    private static readonly Stopwatch _stopwatch = new();
    private static double _lastElapsed;

    public static void Register(SpringState spring)
    {
        _activeSprings.Add(spring);
        if (!_isRunning)
        {
            _isRunning = true;
            _stopwatch.Restart();
            _lastElapsed = 0;
            Dispatcher.UIThread.Post(Tick, DispatcherPriority.Input);
        }
    }

    private static void Tick()
    {
        if (_activeSprings.Count == 0)
        {
            _isRunning = false;
            _stopwatch.Stop();
            return;
        }

        double currentElapsed = _stopwatch.Elapsed.TotalSeconds;
        double dt = Math.Min(currentElapsed - _lastElapsed, 0.1);
        _lastElapsed = currentElapsed;

        var springs = _activeSprings.ToList();
        
        // Batch all updates together
        Sve.Batch(() =>
        {
            foreach (var spring in springs)
            {
                if (!spring.InternalStep(dt))
                {
                    _activeSprings.Remove(spring);
                }
            }
        });

        if (_activeSprings.Count > 0)
        {
            Dispatcher.UIThread.Post(Tick, DispatcherPriority.Input);
        }
        else
        {
            _isRunning = false;
            _stopwatch.Stop();
        }
    }
}
