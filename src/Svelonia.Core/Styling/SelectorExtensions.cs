using Avalonia.Controls;
using Avalonia.Styling;

namespace Svelonia.Core.Styling;

/// <summary>
/// 
/// </summary>
public static class SelectorExtensions
{
    // --- Pseudo Classes (State) ---

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Selector IsPressed(this Selector selector)
        => selector.Class(":pressed");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Selector IsPointerOver(this Selector selector)
        => selector.Class(":pointerover");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Selector IsFocused(this Selector selector)
        => selector.Class(":focus");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Selector IsDisabled(this Selector selector)
        => selector.Class(":disabled");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Selector IsSelected(this Selector selector)
        => selector.Class(":selected");

    // --- Template Drill-down (Drill-down) ---

    /// <summary>
    /// Enter the control template and find elements inside (corresponding to /template/ T)
    /// </summary>
    public static Selector InsideTemplate<T>(this Selector selector) where T : Control
    {
        // Here the logic encapsulates Avalonia's template lookup syntax
        // If Avalonia's syntax changes in the future, just change it here
        return selector.Template().OfType<T>();
    }

    /// <summary>
    /// Enter the template and find an element named partName (corresponding to /template/ #partName)
    /// </summary>
    public static Selector InsideTemplate(this Selector selector, string partName)
    {
        return selector.Template().Name(partName);
    }

    // --- Child Selection (Hierarchy) ---

    /// <summary>
    /// Find direct child elements (corresponding to > T)
    /// </summary>
    public static Selector Child<T>(this Selector selector) where T : Control
    {
        return selector.Child().OfType<T>();
    }
}