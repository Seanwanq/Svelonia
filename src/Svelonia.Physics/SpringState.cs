using System;
using System.Diagnostics;
using Avalonia.Threading;
using Svelonia.Core;

namespace Svelonia.Physics;

/// <summary>
/// A reactive state that smoothly transitions to its target value using spring physics.
/// </summary>
public class SpringState : State<double>, IDisposable
{
    private readonly Spring _simulator;
    private bool _isRunning;
    private readonly Stopwatch _stopwatch = new();
    private double _lastElapsed;

    /// <summary>
    /// Tension of the spring. Default 170.
    /// </summary>
    public double Stiffness { get => _simulator.Stiffness; set => _simulator.Stiffness = value; }

    /// <summary>
    /// Friction of the spring. Default 26.
    /// </summary>
    public double Damping { get => _simulator.Damping; set => _simulator.Damping = value; }

    /// <summary>
    /// Mass of the object. Default 1.
    /// </summary>
    public double Mass { get => _simulator.Mass; set => _simulator.Mass = value; }

    public SpringState(double initialValue) : base(initialValue)
    {
        _simulator = new Spring(initialValue);
    }

    public override double Value
    {
        get => base.Value;
        set
        {
            if (Math.Abs(_simulator.Target - value) < 0.0001) return;
            _simulator.Target = value;
            Start();
        }
    }

    /// <summary>
    /// Sets the value instantly without animation.
    /// </summary>
    public void SetImmediate(double value)
    {
        _simulator.Target = value;
        _simulator.Value = value;
        _simulator.Velocity = 0;
        _isRunning = false;
        base.Value = value;
    }

    private void Start()
    {
        if (_isRunning) return;
        _isRunning = true;
        _stopwatch.Restart();
        _lastElapsed = 0;
        Dispatcher.UIThread.Post(Tick, DispatcherPriority.Render);
    }

    private void Tick()
    {
        if (!_isRunning) return;

        double currentElapsed = _stopwatch.Elapsed.TotalSeconds;
        double dt = currentElapsed - _lastElapsed;
        _lastElapsed = currentElapsed;

        // Cap dt to avoid huge jumps on lag
        if (dt > 0.1) dt = 0.1;

        if (_simulator.Step(dt))
        {
            base.ForceUpdate(_simulator.Value);
            Dispatcher.UIThread.Post(Tick, DispatcherPriority.Render);
        }
        else
        {
            base.ForceUpdate(_simulator.Value);
            _isRunning = false;
            _stopwatch.Stop();
        }
    }

    public void Dispose()
    {
        _isRunning = false;
        _stopwatch.Stop();
    }
}
