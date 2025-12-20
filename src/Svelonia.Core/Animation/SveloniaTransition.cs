using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;

namespace Svelonia.Core.Animation;

/// <summary>
/// 
/// </summary>
public class SveloniaTransition
{
    /// <summary>
    /// 
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(300);

    /// <summary>
    /// 
    /// </summary>
    public Easing? Easing { get; set; }

    /// <summary>
    /// Sets the initial state of the control before the enter animation starts.
    /// Also used as the target state for the exit animation.
    /// </summary>
    public Action<Control>? ApplyInitialState { get; set; }

    /// <summary>
    /// Sets the active state of the control (the state it should be in when visible).
    /// </summary>
    public Action<Control>? ApplyActiveState { get; set; }

    /// <summary>
    /// Creates the Avalonia Transitions required to animate between states.
    /// </summary>
    public Func<Transitions> CreateTransitions { get; set; } = () => new Transitions();
}
