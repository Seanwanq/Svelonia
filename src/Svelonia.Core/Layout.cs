using Avalonia.Controls;

namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public abstract class Layout : Component
{
    /// <summary>
    /// 
    /// </summary>
    protected Control Slot { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    public Layout(Control slot)
    {
        Slot = slot;
    }
}