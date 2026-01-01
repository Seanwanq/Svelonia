using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// Atomic utility extensions for Svelonia (Tailwind-like)
/// </summary>
public static class AtomicStyleExtensions
{
    /// <summary>
    /// Background
    /// </summary>
    public static T Bg<T>(this T control, object brush, object? hover = null, object? pressed = null) where T : Control
    {
        var prop = control is Panel ? Panel.BackgroundProperty : 
                   control is Avalonia.Controls.Border ? Avalonia.Controls.Border.BackgroundProperty :
                   control is TemplatedControl ? TemplatedControl.BackgroundProperty :
                   control is ContentPresenter ? ContentPresenter.BackgroundProperty :
                   null;

        if (prop != null)
            control.SetStateProperty(prop, brush, hover, pressed);
            
        return control;
    }

    /// <summary>
    /// Text Color
    /// </summary>
    public static T Fg<T>(this T control, object brush, object? hover = null, object? pressed = null) where T : Control
    {
        var prop = control is TemplatedControl ? TemplatedControl.ForegroundProperty :
                   control is TextBlock ? TextBlock.ForegroundProperty :
                   control is ContentPresenter ? ContentPresenter.ForegroundProperty :
                   null;

        if (prop != null)
            control.SetStateProperty(prop, brush, hover, pressed);
            
        return control;
    }

    /// <summary>
    /// Rounded Corners
    /// </summary>
    public static T Rounded<T>(this T control, double units = 1, double? hover = null, double? pressed = null) where T : Control
    {
        var normal = new CornerRadius(AtomicTheme.GetSize(units));
        var h = hover.HasValue ? new CornerRadius(AtomicTheme.GetSize(hover.Value)) : (object?)null;
        var p = pressed.HasValue ? new CornerRadius(AtomicTheme.GetSize(pressed.Value)) : (object?)null;
        
        // Use SetStateProperty directly as SetCornerRadius might be specific to Border/TemplatedControl
        var prop = control is Avalonia.Controls.Border ? Avalonia.Controls.Border.CornerRadiusProperty : 
                   control is TemplatedControl ? TemplatedControl.CornerRadiusProperty : null;
                   
        if (prop != null)
            control.SetStateProperty(prop, normal, h, p);
            
        return control;
    }

    /// <summary>
    /// Fully Rounded Corners (Pill shape)
    /// </summary>
    public static T RoundedFull<T>(this T control) where T : Control
        => control.Rounded(9999);

    /// <summary>
    /// Padding
    /// </summary>
    public static T P<T>(this T control, double uniform, double? hover = null, double? pressed = null) where T : Control
    {
        var normal = new Thickness(AtomicTheme.GetSize(uniform));
        var h = hover.HasValue ? new Thickness(AtomicTheme.GetSize(hover.Value)) : (object?)null;
        var p = pressed.HasValue ? new Thickness(AtomicTheme.GetSize(pressed.Value)) : (object?)null;
        
        var prop = control is Decorator ? Decorator.PaddingProperty :
                   control is TemplatedControl ? TemplatedControl.PaddingProperty : 
                   control is TextBlock ? TextBlock.PaddingProperty : null;

        if (prop != null)
            control.SetStateProperty(prop, normal, h, p);
            
        return control;
    }

    /// <summary>
    /// Horizontal and Vertical Padding
    /// </summary>
    public static T P<T>(this T control, double horizontal, double vertical) where T : Control
    {
        var t = new Thickness(AtomicTheme.GetSize(horizontal), AtomicTheme.GetSize(vertical));
        if (control is Decorator d) d.Padding = t;
        else if (control is TemplatedControl tc) tc.Padding = t;
        else if (control is TextBlock tb) tb.Padding = t;
        return control;
    }

    /// <summary>
    /// Left, Top, Right, Bottom Padding
    /// </summary>
    public static T P<T>(this T control, double left, double top, double right, double bottom) where T : Control
    {
        var t = new Thickness(AtomicTheme.GetSize(left), AtomicTheme.GetSize(top), AtomicTheme.GetSize(right), AtomicTheme.GetSize(bottom));
        if (control is Decorator d) d.Padding = t;
        else if (control is TemplatedControl tc) tc.Padding = t;
        else if (control is TextBlock tb) tb.Padding = t;
        return control;
    }

    /// <summary>
    /// Margin
    /// </summary>
    public static T M<T>(this T control, double uniform) where T : Control
    {
        control.Margin = new Thickness(AtomicTheme.GetSize(uniform));
        return control;
    }

    /// <summary>
    /// Horizontal and Vertical Margin
    /// </summary>
    public static T M<T>(this T control, double horizontal, double vertical) where T : Control
    {
        control.Margin = new Thickness(AtomicTheme.GetSize(horizontal), AtomicTheme.GetSize(vertical));
        return control;
    }

    /// <summary>
    /// Border Thickness
    /// </summary>
        /// <summary>
        /// Multi-state BoxShadow (Only for Border)
        /// </summary>
        public static T SetBoxShadow<T>(this T control, object? normal, object? hover = null, object? pressed = null, object? disabled = null, object? focus = null) where T : Control
        {
            if (control is not Avalonia.Controls.Border border) return control;
            var prop = Avalonia.Controls.Border.BoxShadowProperty;
    
            if (hover == null && pressed == null && disabled == null && focus == null)
            {
                control.ApplySingleState(prop, normal);
                return control;
            }
    
            return control.SetStateProperty(prop, normal, hover, pressed, disabled, focus);
        }
    }
