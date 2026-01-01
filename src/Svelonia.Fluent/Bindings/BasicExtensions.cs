using Avalonia.Controls;

namespace Svelonia.Fluent;

public static class BasicExtensions
{
    /// <summary>
    /// Sets the Text property (alias to avoid collision with Text property).
    /// </summary>
    public static T TextContent<T>(this T control, string text) where T : TextBlock
    {
        control.Text = text;
        return control;
    }
    
    /// <summary>
    /// Sets the Spacing property (alias to avoid collision).
    /// </summary>
    public static T Gap<T>(this T control, double spacing) where T : StackPanel
    {
        control.Spacing = spacing;
        return control;
    }
}