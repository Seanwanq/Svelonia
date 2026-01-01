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

    /// <summary>
    /// Sets the Content property (alias to avoid collision).
    /// </summary>
    public static T Child<T>(this T control, object content) where T : ContentControl
    {
        control.Content = content;
        return control;
    }

    /// <summary>
    /// Sets the Child property for Decorator.
    /// </summary>
    public static T Child<T>(this T control, Control child) where T : Decorator
    {
        control.Child = child;
        return control;
    }

    /// <summary>
    /// Adds children to the panel.
    /// </summary>
    public static T Children<T>(this T panel, params Control[] children) where T : Panel
    {
        panel.Children.AddRange(children);
        return panel;
    }
}
