using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Svelonia.Core.Styling;

/// <summary>
/// 
/// </summary>
public static class SpacingExtensions_Layoutable
{
    // --- MARGIN (All Layoutable) ---

    /// <summary>
    /// Margin: All
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T M<T>(this T control, double units) where T : Layoutable
    {
        control.Margin = new Thickness(AtomicTheme.GetSize(units));
        return control;
    }

    /// <summary>
    /// Margin: Horizontal (Left and Right)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Mx<T>(this T control, double units) where T : Layoutable
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Margin;
        control.Margin = new Thickness(val, cur.Top, val, cur.Bottom);
        return control;
    }

    /// <summary>
    /// Margin: Vertical (Top and Bottom)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T My<T>(this T control, double units) where T : Layoutable
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Margin;
        control.Margin = new Thickness(cur.Left, val, cur.Right, val);
        return control;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Mt<T>(this T control, double units) where T : Layoutable
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Margin;
        control.Margin = new Thickness(cur.Left, val, cur.Right, cur.Bottom);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Mb<T>(this T control, double units) where T : Layoutable
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Margin;
        control.Margin = new Thickness(cur.Left, cur.Top, cur.Right, val);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Ml<T>(this T control, double units) where T : Layoutable
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Margin;
        control.Margin = new Thickness(val, cur.Top, cur.Right, cur.Bottom);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Mr<T>(this T control, double units) where T : Layoutable
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Margin;
        control.Margin = new Thickness(cur.Left, cur.Top, val, cur.Bottom);
        return control;
    }
}

/// <summary>
/// 
/// </summary>
public static class SpacingExtensions_Decorator
{
    // --- PADDING (Decorator - e.g. Border) ---

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T P<T>(this T control, double units) where T : Decorator
    {
        control.Padding = new Thickness(AtomicTheme.GetSize(units));
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Px<T>(this T control, double units) where T : Decorator
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Padding;
        control.Padding = new Thickness(val, cur.Top, val, cur.Bottom);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Py<T>(this T control, double units) where T : Decorator
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Padding;
        control.Padding = new Thickness(cur.Left, val, cur.Right, val);
        return control;
    }
}

/// <summary>
/// 
/// </summary>
public static class SpacingExtensions_TemplatedControl
{
    // --- PADDING (TemplatedControl - e.g. Button, TextBox) ---

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T P<T>(this T control, double units) where T : TemplatedControl
    {
        control.Padding = new Thickness(AtomicTheme.GetSize(units));
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Px<T>(this T control, double units) where T : TemplatedControl
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Padding;
        control.Padding = new Thickness(val, cur.Top, val, cur.Bottom);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Py<T>(this T control, double units) where T : TemplatedControl
    {
        var val = AtomicTheme.GetSize(units);
        var cur = control.Padding;
        control.Padding = new Thickness(cur.Left, val, cur.Right, val);
        return control;
    }
}