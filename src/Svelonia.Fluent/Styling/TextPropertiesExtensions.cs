using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Svelonia.Fluent;

public static class TextPropertiesExtensions
{
    public static T FontSize<T>(this T control, double size) where T : Control
    {
        if (control is TemplatedControl tc) tc.FontSize = size;
        else if (control is TextBlock tb) tb.FontSize = size;
        return control;
    }

    public static T FontWeight<T>(this T control, FontWeight weight) where T : Control
    {
         if (control is TemplatedControl tc) tc.FontWeight = weight;
        else if (control is TextBlock tb) tb.FontWeight = weight;
        return control;
    }
}
