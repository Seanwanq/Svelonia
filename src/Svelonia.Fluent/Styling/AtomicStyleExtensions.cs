using Avalonia;
using Avalonia.Controls;
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
        => control.Background(brush, hover, pressed);

    /// <summary>
    /// Text Color
    /// </summary>
    public static T Fg<T>(this T control, object brush, object? hover = null, object? pressed = null) where T : Control
        => control.Foreground(brush, hover, pressed);

    /// <summary>
    /// Rounded Corners
    /// </summary>
    public static T Rounded<T>(this T control, double units = 1, double? hover = null, double? pressed = null) where T : Control
    {
        var normal = new CornerRadius(AtomicTheme.GetSize(units));
        var h = hover.HasValue ? new CornerRadius(AtomicTheme.GetSize(hover.Value)) : (object?)null;
        var p = pressed.HasValue ? new CornerRadius(AtomicTheme.GetSize(pressed.Value)) : (object?)null;
        return control.CornerRadius(normal, h, p);
    }

    /// <summary>
    /// Fully Rounded Corners (Pill shape)
    /// </summary>
    public static T RoundedFull<T>(this T control) where T : Control
        => control.CornerRadius(new CornerRadius(9999));

    /// <summary>
    /// Padding
    /// </summary>
    public static T P<T>(this T control, double uniform, double? hover = null, double? pressed = null) where T : Control
    {
        var normal = new Thickness(AtomicTheme.GetSize(uniform));
        var h = hover.HasValue ? new Thickness(AtomicTheme.GetSize(hover.Value)) : (object?)null;
        var p = pressed.HasValue ? new Thickness(AtomicTheme.GetSize(pressed.Value)) : (object?)null;
        return control.Padding(normal, h, p);
    }

    /// <summary>
    /// Horizontal and Vertical Padding
    /// </summary>
    public static T P<T>(this T control, double horizontal, double vertical) where T : Control
        => control.Padding(new Thickness(AtomicTheme.GetSize(horizontal), AtomicTheme.GetSize(vertical)));

    /// <summary>
    /// Left, Top, Right, Bottom Padding
    /// </summary>
    public static T P<T>(this T control, double left, double top, double right, double bottom) where T : Control
        => control.Padding(new Thickness(AtomicTheme.GetSize(left), AtomicTheme.GetSize(top), AtomicTheme.GetSize(right), AtomicTheme.GetSize(bottom)));

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
    public static T Border<T>(this T control, double uniform, object? hover = null, object? pressed = null) where T : Control
    {
        var normal = new Thickness(uniform);
        return control.BorderThickness(normal, hover, pressed);
    }
}
