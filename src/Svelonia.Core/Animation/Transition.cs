using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace Svelonia.Core.Animation;

/// <summary>
/// 
/// </summary>
public static class Transition
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public static SveloniaTransition Fade(double duration = 300)
    {
        var ts = TimeSpan.FromMilliseconds(duration);
        return new SveloniaTransition
        {
            Duration = ts,
            ApplyInitialState = c => c.Opacity = 0,
            ApplyActiveState = c => c.Opacity = 1,
            CreateTransitions = () => new Transitions
            {
                new DoubleTransition
                {
                    Property = Visual.OpacityProperty,
                    Duration = ts
                }
            }
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static SveloniaTransition Fly(double duration = 300, double x = 0, double y = 0)
    {
        var ts = TimeSpan.FromMilliseconds(duration);

        return new SveloniaTransition
        {
            Duration = ts,
            ApplyInitialState = c =>
            {
                c.Opacity = 0;
                c.RenderTransform = new TranslateTransform(x, y);
            },
            ApplyActiveState = c =>
            {
                c.Opacity = 1;
                c.RenderTransform = new TranslateTransform(0, 0);
            },
            CreateTransitions = () => new Transitions
            {
                new DoubleTransition
                {
                    Property = Visual.OpacityProperty,
                    Duration = ts
                },
                new TransformOperationsTransition
                {
                    Property = Visual.RenderTransformProperty,
                    Duration = ts,
                    Easing = new CubicEaseOut()
                }
            }
        };
    }

    /// <summary>
    /// Scale, Slide etc. can be added similarly
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="startScale"></param>
    /// <returns></returns>
    public static SveloniaTransition Scale(double duration = 300, double startScale = 0.95)
    {
        var ts = TimeSpan.FromMilliseconds(duration);
        return new SveloniaTransition
        {
            Duration = ts,
            ApplyInitialState = c =>
            {
                c.Opacity = 0;
                c.RenderTransform = new ScaleTransform(startScale, startScale);
            },
            ApplyActiveState = c =>
            {
                c.Opacity = 1;
                c.RenderTransform = new ScaleTransform(1, 1);
            },
            CreateTransitions = () => new Transitions
            {
                new DoubleTransition { Property = Visual.OpacityProperty, Duration = ts },
                new TransformOperationsTransition { Property = Visual.RenderTransformProperty, Duration = ts, Easing = new CubicEaseOut() }
            }
        };
    }
}
