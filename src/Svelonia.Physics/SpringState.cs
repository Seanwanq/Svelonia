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

    /// <summary>
    /// The target value the spring is moving towards.
    /// </summary>
    public State<double> TargetState { get; }

    private bool _isSleeping;

    public SpringState(double initialValue) : base(initialValue)
    {
        TargetState = new State<double>(initialValue);
        _simulator = new Spring(initialValue);
    }

    public override double Value
    {
        get => base.Value;
        set
        {
            if (Math.Abs(_simulator.Target - value) < 0.0001) return;
            _simulator.Target = value;
            TargetState.Value = value;
            
            if (_isSleeping)
            {
                // If sleeping, apply immediately (no animation)
                _simulator.Value = value;
                _simulator.Velocity = 0;
                base.Value = value;
            }
            else
            {
                Start();
            }
        }
    }

    /// <summary>
    /// Puts the spring to sleep.
    /// While sleeping, any value changes are applied immediately (teleport) without animation.
    /// This removes the spring from the physics loop, saving CPU.
    /// </summary>
    public void Sleep()
    {
        if (_isSleeping) return;
        _isSleeping = true;
        
        // Ensure we are at target before sleeping
        SetImmediate(TargetState.Value);
        
        // Remove from loop (handled by SetImmediate -> Velocity=0 -> AnimationLoop will drop it naturally? 
        // No, AnimationLoop keeps it until InternalStep returns false.
        // But InternalStep returns false if resting.
        // SetImmediate makes it resting.
        // So effectively, we just need to ensure it's resting.
    }

    /// <summary>
    /// Wakes the spring up. Future changes will be animated.
    /// </summary>
    public void Wake()
    {
        if (!_isSleeping) return;
        _isSleeping = false;
        
        // If we somehow drifted or target changed, start animating
        if (Math.Abs(_simulator.Value - _simulator.Target) > 0.001)
        {
            Start();
        }
    }

    /// <summary>
    /// Sets the value instantly without animation.
    /// </summary>
    public void SetImmediate(double value)
    {
        _simulator.Target = value;
        TargetState.Value = value;
        _simulator.Value = value;
        _simulator.Velocity = 0;
        base.Value = value;
    }

    private void Start()
    {
        AnimationLoop.Register(this);
    }

    /// <summary>
    /// Performs a single step of simulation. Called by AnimationLoop.
    /// </summary>
    /// <returns>True if still moving.</returns>
    internal bool InternalStep(double dt)
    {
        if (_simulator.Step(dt))
        {
            if (!double.IsFinite(_simulator.Value)) return false;
            base.ForceUpdate(_simulator.Value);
            return true;
        }
        else
        {
            if (double.IsFinite(_simulator.Value))
                base.ForceUpdate(_simulator.Value);
            return false;
        }
    }

    public void Dispose()
    {
    }
}
