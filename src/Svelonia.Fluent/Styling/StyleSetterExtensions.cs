using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace Svelonia.Fluent;

/// <summary>
/// Fluent extensions for Avalonia.Styling.Style to add setters easily.
/// </summary>
public static class StyleSetterExtensions
{
    /// <summary>
    /// Adds a setter for a property.
    /// </summary>
    public static Style Setter(this Style style, AvaloniaProperty property, object value)
    {
        style.Setters.Add(new Setter(property, value));
        return style;
    }

    /// <summary>
    /// 
    /// </summary>
    public static Style Background(this Style style, IBrush brush) => style.Setter(TemplatedControl.BackgroundProperty, brush);

    /// <summary>
    /// 
    /// </summary>
    public static Style Background(this Style style, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) => style.Setter(TemplatedControl.BackgroundProperty, resource);

    /// <summary>
    /// 
    /// </summary>
    public static Style Foreground(this Style style, IBrush brush) => style.Setter(TemplatedControl.ForegroundProperty, brush);

    /// <summary>
    /// 
    /// </summary>
    public static Style Foreground(this Style style, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) => style.Setter(TemplatedControl.ForegroundProperty, resource);

    /// <summary>
    /// 
    /// </summary>
    public static Style FontSize(this Style style, double size) => style.Setter(TemplatedControl.FontSizeProperty, size);

    /// <summary>
    /// 
    /// </summary>
    public static Style HorizontalAlignment(this Style style, HorizontalAlignment alignment) => style.Setter(Layoutable.HorizontalAlignmentProperty, alignment);

    /// <summary>
    /// 
    /// </summary>
    public static Style VerticalAlignment(this Style style, VerticalAlignment alignment) => style.Setter(Layoutable.VerticalAlignmentProperty, alignment);

    /// <summary>
    /// 
    /// </summary>
    public static Style HorizontalContentAlignment(this Style style, HorizontalAlignment alignment) => style.Setter(ContentControl.HorizontalContentAlignmentProperty, alignment);

    /// <summary>
    /// 
    /// </summary>
    public static Style VerticalContentAlignment(this Style style, VerticalAlignment alignment) => style.Setter(ContentControl.VerticalContentAlignmentProperty, alignment);

    /// <summary>
    /// 
    /// </summary>
    public static Style Padding(this Style style, double uniform) => style.Setter(TemplatedControl.PaddingProperty, new Thickness(uniform));

    /// <summary>
    /// 
    /// </summary>
    public static Style Padding(this Style style, double horizontal, double vertical) => style.Setter(TemplatedControl.PaddingProperty, new Thickness(horizontal, vertical));

    /// <summary>
    /// 
    /// </summary>
    public static Style Padding(this Style style, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) => style.Setter(TemplatedControl.PaddingProperty, resource);

    /// <summary>
    /// 
    /// </summary>
    public static Style Margin(this Style style, double uniform) => style.Setter(Layoutable.MarginProperty, new Thickness(uniform));

    /// <summary>
    /// 
    /// </summary>
    public static Style Margin(this Style style, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) => style.Setter(Layoutable.MarginProperty, resource);

    /// <summary>
    /// 
    /// </summary>
    public static Style CornerRadius(this Style style, double uniform) => style.Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(uniform));

    /// <summary>
    /// 
    /// </summary>
    public static Style CornerRadius(this Style style, Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension resource) => style.Setter(TemplatedControl.CornerRadiusProperty, resource);

    /// <summary>
    /// 
    /// </summary>
    public static Style Opacity(this Style style, double value) => style.Setter(Visual.OpacityProperty, value);

    /// <summary>
    /// Adds transitions to the style.
    /// </summary>
    public static Style Transitions(this Style style, params ITransition[] transitions)
    {
        var collection = new Transitions();
        collection.AddRange(transitions);
        return style.Setter(Control.TransitionsProperty, collection);
    }
}