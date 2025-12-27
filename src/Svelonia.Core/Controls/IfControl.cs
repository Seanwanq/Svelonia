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
    private readonly Func<Control>? _elseBuilder;
    private readonly SveloniaTransition? _transition;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="builder"></param>
    /// <param name="elseBuilder"></param>
    /// <param name="animate"></param>
    /// <param name="transition"></param>
    public IfControl(State<bool> condition, Func<Control> builder, Func<Control>? elseBuilder = null, bool animate = false, SveloniaTransition? transition = null)
    {
        _condition = condition;
        _builder = builder;
        _elseBuilder = elseBuilder;

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
            // Show Primary View
            var control = _builder();
            ApplyTransition(control);
            Content = control;
        }
        else
        {
            // Show Else View or Clear
            if (_elseBuilder != null)
            {
                var control = _elseBuilder();
                ApplyTransition(control);
                Content = control;
            }
            else
            {
                Content = null;
            }
        }
    }

    private async void ApplyTransition(Control control)
    {
        if (_transition != null)
        {
            _transition.ApplyInitialState?.Invoke(control);
            control.Transitions = _transition.CreateTransitions();
            await Task.Yield();
            _transition.ApplyActiveState?.Invoke(control);
        }
    }
}
