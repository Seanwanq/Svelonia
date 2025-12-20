using Avalonia.Layout;

using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
public static class SizingExtensions
{
    /// <summary>
    /// Width
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T W<T>(this T control, double units) where T : Layoutable
    {
        control.Width = AtomicTheme.GetSize(units);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T MinW<T>(this T control, double units) where T : Layoutable
    {
        control.MinWidth = AtomicTheme.GetSize(units);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T MaxW<T>(this T control, double units) where T : Layoutable
    {
        control.MaxWidth = AtomicTheme.GetSize(units);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <returns></returns>
    public static T WFull<T>(this T control) where T : Layoutable
    {
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        return control;
    }

    /// <summary>
    /// Height
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T H<T>(this T control, double units) where T : Layoutable
    {
        control.Height = AtomicTheme.GetSize(units);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T MinH<T>(this T control, double units) where T : Layoutable
    {
        control.MinHeight = AtomicTheme.GetSize(units);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    public static T MaxH<T>(this T control, double units) where T : Layoutable
    {
        control.MaxHeight = AtomicTheme.GetSize(units);
        return control;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <returns></returns>
    public static T HFull<T>(this T control) where T : Layoutable
    {
        control.VerticalAlignment = VerticalAlignment.Stretch;
        return control;
    }

    /// <summary>
    /// Full Screen (Both)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control"></param>
    /// <returns></returns>
    public static T Full<T>(this T control) where T : Layoutable
    {
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.VerticalAlignment = VerticalAlignment.Stretch;
        return control;
    }
}