using Avalonia.Controls;

namespace Svelonia.Fluent;

public static class CanvasExtensions
{
    public static T Left<T>(this T control, double value) where T : Control
    {
        Canvas.SetLeft(control, value);
        return control;
    }

    public static T Top<T>(this T control, double value) where T : Control
    {
        Canvas.SetTop(control, value);
        return control;
    }
    
    public static T Right<T>(this T control, double value) where T : Control
    {
        Canvas.SetRight(control, value);
        return control;
    }

    public static T Bottom<T>(this T control, double value) where T : Control
    {
        Canvas.SetBottom(control, value);
        return control;
    }
}
