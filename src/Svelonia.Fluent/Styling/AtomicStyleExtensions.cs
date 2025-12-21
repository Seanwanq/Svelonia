using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
public static class StyleHelpers_TemplatedControl
{
    /// <summary>
    /// Backgroud
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="brush"></param>
    /// <returns></returns>
    public static T Bg<T>(this T control, IBrush brush) where T : TemplatedControl
    {
        control.Background = brush;
        return control;
    }

    /// <summary>
    /// Backgroud (Dynamic Resource)
    /// </summary>
    public static T Bg<T>(this T control, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) where T : TemplatedControl
    {
        control.Bind(TemplatedControl.BackgroundProperty, resource);
        return control;
    }

    /// <summary>
    /// Text Color
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="brush"></param>
    /// <returns></returns>
    public static T Fg<T>(this T control, IBrush brush) where T : TemplatedControl
    {
        control.Foreground = brush;
        return control;
    }

    /// <summary>
    /// Text Color (Dynamic Resource)
    /// </summary>
    public static T Fg<T>(this T control, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) where T : TemplatedControl
    {
        control.Bind(TemplatedControl.ForegroundProperty, resource);
        return control;
    }
}

/// <summary>
/// 
/// </summary>
public static class StyleHelpers_Panel
{
    /// <summary>
    /// Backgroud
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="brush"></param>
    /// <returns></returns>
    public static T Bg<T>(this T control, IBrush brush) where T : Panel
    {
        control.Background = brush;
        return control;
    }

    /// <summary>
    /// Backgroud (Dynamic Resource)
    /// </summary>
    public static T Bg<T>(this T control, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) where T : Panel
    {
        control.Bind(Panel.BackgroundProperty, resource);
        return control;
    }
}

/// <summary>
/// 
/// </summary>
public static class StyleHelpers_Border
{
    /// <summary>
    /// Backgroud
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="brush"></param>
    /// <returns></returns>
    public static T Bg<T>(this T control, IBrush brush) where T : Border
    {
        control.Background = brush;
        return control;
    }

    /// <summary>
    /// Backgroud (Dynamic Resource)
    /// </summary>
    public static T Bg<T>(this T control, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) where T : Border
    {
        control.Bind(Border.BackgroundProperty, resource);
        return control;
    }

    /// <summary>
    /// Rounded Corners
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T Rounded<T>(this T control, double units = 1) where T : Border
    {
        control.CornerRadius = new CornerRadius(AtomicTheme.GetSize(units));
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <returns></returns>
    public static T RoundedFull<T>(this T control) where T : Border
    {
        control.CornerRadius = new CornerRadius(9999);
        return control;
    }
}

/// <summary>
/// 
/// </summary>
public static class StyleHelpers_TextBlock
{
    /// <summary>
    /// Text Color
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="brush"></param>
    /// <returns></returns>
    public static T Fg<T>(this T control, IBrush brush) where T : TextBlock
    {
        control.Foreground = brush;
        return control;
    }

    /// <summary>
    /// Text Color (Dynamic Resource)
    /// </summary>
    public static T Fg<T>(this T control, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) where T : TextBlock
    {
        control.Bind(TextBlock.ForegroundProperty, resource);
        return control;
    }
}