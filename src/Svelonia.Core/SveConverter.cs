using Avalonia;
using Avalonia.Media;

namespace Svelonia.Core;

/// <summary>
/// Internal utility to handle automatic type conversions for Svelonia Fluent API.
/// </summary>
public static class SveConverter
{
    public static object? Convert(AvaloniaProperty prop, object? value)
    {
        if (value == null) return null;

        var targetType = prop.PropertyType;
        var valueType = value.GetType();

        if (targetType.IsAssignableFrom(valueType)) return value;

        // Numeric to Thickness
        if (targetType == typeof(Thickness))
        {
            if (value is double d) return new Thickness(d);
            if (value is int i) return new Thickness(i);
            if (value is float f) return new Thickness(f);
        }

        // Numeric to CornerRadius
        if (targetType == typeof(CornerRadius))
        {
            if (value is double d) return new CornerRadius(d);
            if (value is int i) return new CornerRadius(i);
            if (value is float f) return new CornerRadius(f);
        }

        // String to Color/Brush
        if (value is string s)
        {
            if (targetType == typeof(Color) && Color.TryParse(s, out var color)) return color;
            if (targetType == typeof(IBrush) && Color.TryParse(s, out var bColor)) return new SolidColorBrush(bColor);
        }

        return value;
    }
}
