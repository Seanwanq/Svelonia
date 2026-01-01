using Avalonia.Layout;

namespace Svelonia.Fluent;

public static class LayoutExtensions
{
    public static T HorizontalAlignment<T>(this T control, HorizontalAlignment align) where T : Layoutable
    {
        control.HorizontalAlignment = align;
        return control;
    }

    public static T VerticalAlignment<T>(this T control, VerticalAlignment align) where T : Layoutable
    {
        control.VerticalAlignment = align;
        return control;
    }
}
