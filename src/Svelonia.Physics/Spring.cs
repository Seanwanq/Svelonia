using System;

namespace Svelonia.Physics;

/// <summary>
/// Core spring physics simulator.
/// Based on damped harmonic oscillator equations.
/// </summary>
public class Spring
{
    /// <summary>
    /// Tension of the spring. Higher means faster movement. Default 170.
    /// </summary>
    public double Stiffness { get; set; } = 170;

    /// <summary>
    /// Friction of the spring. Higher means less bounciness. Default 26.
    /// </summary>
    public double Damping { get; set; } = 26;

    /// <summary>
    /// Mass of the object. Higher means more inertia. Default 1.
    /// </summary>
    public double Mass { get; set; } = 1;

    /// <summary>
    /// Current velocity of the object.
    /// </summary>
    public double Velocity { get; set; }

    /// <summary>
    /// Current position/value of the object.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// The target destination.
    /// </summary>
    public double Target { get; set; }

    private double _accumulator;
    private const double SOLVER_TIMESTEP = 0.001; // 1ms physics step
    private const double MAX_DT = 0.064; // Max 64ms per frame to prevent spiral of death

    public Spring(double initialValue)
    {
        Value = initialValue;
        Target = initialValue;
    }

    /// <summary>
    /// Updates the spring state by one time step using robust sub-stepping.
    /// </summary>
    /// <param name="deltaTime">Time since last update in seconds.</param>
    /// <returns>True if the spring is still moving.</returns>
    public bool Step(double deltaTime)
    {
        // Cap large time steps to prevent instability
        if (deltaTime > MAX_DT) deltaTime = MAX_DT;

        _accumulator += deltaTime;

        // Consume time in fixed small steps
        while (_accumulator >= SOLVER_TIMESTEP)
        {
            Integrate(SOLVER_TIMESTEP);
            _accumulator -= SOLVER_TIMESTEP;
        }

        // Check for rest state (arbitrary small thresholds)
        bool isResting = Math.Abs(Velocity) < 0.005 && Math.Abs(Value - Target) < 0.005;
        if (isResting)
        {
            Value = Target;
            Velocity = 0;
            _accumulator = 0;
        }

        return !isResting;
    }

    private void Integrate(double dt)
    {
        // F = -k * x - c * v
        double displacement = Value - Target;
        double springForce = -Stiffness * displacement;
        double dampingForce = -Damping * Velocity;
        double totalForce = springForce + dampingForce;

        // a = F / m
        double acceleration = totalForce / Mass;

        // Semi-Implicit Euler (More stable than standard Euler)
        // 1. Update Velocity first
        Velocity += acceleration * dt;
        
        // 2. Update Position using the NEW Velocity
        Value += Velocity * dt;
    }
}
