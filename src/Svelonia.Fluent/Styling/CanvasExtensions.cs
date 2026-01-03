using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class CanvasExtensions
{
    public static T SetLeft<T>(this T control, double value) where T : Control
    {
        Canvas.SetLeft(control, value);
        return control;
    }

    public static T SetTop<T>(this T control, double value) where T : Control
    {
        Canvas.SetTop(control, value);
        return control;
    }
    
    public static T SetRight<T>(this T control, double value) where T : Control
    {
        Canvas.SetRight(control, value);
        return control;
    }

    public static T SetBottom<T>(this T control, double value) where T : Control
    {
        Canvas.SetBottom(control, value);
        return control;
    }

    public static T BindLeft<T>(this T control, State<double> state) where T : Control
    {
        control.Bind(Canvas.LeftProperty, state.ToBinding());
        return control;
    }

    public static T BindTop<T>(this T control, State<double> state) where T : Control
    {
        control.Bind(Canvas.TopProperty, state.ToBinding());
        return control;
    }

    public static T BindRight<T>(this T control, State<double> state) where T : Control
    {
        control.Bind(Canvas.RightProperty, state.ToBinding());
        return control;
    }

    public static T BindBottom<T>(this T control, State<double> state) where T : Control
    {
        control.Bind(Canvas.BottomProperty, state.ToBinding());
        return control;
    }
}
