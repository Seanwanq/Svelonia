using Avalonia;
using Avalonia.Controls;
using Svelonia.Core.Animation;

namespace Svelonia.Core.Controls;

/// <summary>
/// 
/// </summary>
public class IfControl : UserControl
{
    private readonly State<bool> _condition;
    private readonly Func<Control> _builder;
    private readonly SveloniaTransition? _transition;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="builder"></param>
    /// <param name="animate"></param>
    /// <param name="transition"></param>
    public IfControl(State<bool> condition, Func<Control> builder, bool animate = false, SveloniaTransition? transition = null)
    {
        _condition = condition;
        _builder = builder;

        if (transition != null)
        {
            _transition = transition;
        }
        else if (animate)
        {
            _transition = Transition.Fade();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _condition.OnChange += Update;
        Update(_condition.Value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _condition.OnChange -= Update;
        Content = null;
    }

    private async void Update(bool value)
    {
        if (value)
        {
            // Show
            if (Content == null)
            {
                var control = _builder();

                if (_transition != null)
                {
                    // 1. Initial State
                    _transition.ApplyInitialState?.Invoke(control);

                    // 2. Setup Transitions
                    control.Transitions = _transition.CreateTransitions();
                }

                Content = control;

                if (_transition != null)
                {
                    // 3. Trigger Active State (Enter Animation)
                    await Task.Yield(); // Allow layout pass
                    _transition.ApplyActiveState?.Invoke(control);
                }
            }
        }
        else
        {
            // Hide
            if (Content != null)
            {
                if (_transition != null && Content is Control control)
                {
                    // 1. Trigger Exit State (Back to Initial)
                    _transition.ApplyInitialState?.Invoke(control);

                    // 2. Wait
                    await Task.Delay(_transition.Duration);
                }

                // Double check condition hasn't flipped back while waiting
                if (!_condition.Value)
                {
                    Content = null;
                }
            }
        }
    }
}
