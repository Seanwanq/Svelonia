using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class MultiTouchDragExtensions
{
    private class LiveDragState
    {
        public Point StartPointerPosition { get; set; }
        public double InitialTransformX { get; set; }
        public double InitialTransformY { get; set; }
        public TranslateTransform Transform { get; set; } = new();
        public bool IsDragging { get; set; }
        public long PointerId { get; set; } = -1;
    }

    /// <summary>
    /// Enables multi-touch concurrent dragging. 
    /// Moves the control itself using RenderTransform.
    /// </summary>
    public static T LiveDraggable<T>(this T control, State<bool>? enable = null, Action<Point>? onMove = null) where T : Control
    {
        var state = new LiveDragState();

        control.PointerPressed += (s, e) =>
        {
            if (enable != null && !enable.Value) return;
            if (state.IsDragging) return;

            var parent = control.Parent as Visual;
            var point = e.GetCurrentPoint(parent);
            
            if (point.Properties.IsLeftButtonPressed)
            {
                EnsureTransform(control, state);
                state.IsDragging = true;
                state.PointerId = point.Pointer.Id;
                state.StartPointerPosition = point.Position;
                state.InitialTransformX = state.Transform.X;
                state.InitialTransformY = state.Transform.Y;

                e.Pointer.Capture(control);
                e.Handled = true;
            }
        };

        control.PointerMoved += (s, e) =>
        {
            if (!state.IsDragging || e.Pointer.Id != state.PointerId) return;

            var parent = control.Parent as Visual;
            var point = e.GetCurrentPoint(parent);
            var currentPos = point.Position;
            
            var deltaX = currentPos.X - state.StartPointerPosition.X;
            var deltaY = currentPos.Y - state.StartPointerPosition.Y;

            state.Transform.X = state.InitialTransformX + deltaX;
            state.Transform.Y = state.InitialTransformY + deltaY;

            // Optional callback for mirroring or linked logic
            onMove?.Invoke(new Point(deltaX, deltaY));
        };

        control.PointerReleased += (s, e) =>
        {
            if (state.IsDragging && e.Pointer.Id == state.PointerId)
            {
                state.IsDragging = false;
                state.PointerId = -1;
                e.Pointer.Capture(null);
                e.Handled = true;
            }
        };

        control.PointerCaptureLost += (s, e) =>
        {
            if (state.IsDragging && e.Pointer.Id == state.PointerId)
            {
                state.IsDragging = false;
                state.PointerId = -1;
                e.Handled = true;
            }
        };

        return control;
    }

    private static void EnsureTransform(Control c, LiveDragState s)
    {
        if (c.RenderTransform is not TranslateTransform tt)
        {
            s.Transform = new TranslateTransform();
            c.RenderTransform = s.Transform;
        }
        else
        {
            s.Transform = tt;
        }
    }

    /// <summary>
    /// Helper to manually move a control (for simulation)
    /// </summary>
    public static void ManualMove(this Control control, double x, double y)
    {
        if (control.RenderTransform is not TranslateTransform tt)
        {
            tt = new TranslateTransform();
            control.RenderTransform = tt;
        }
        tt.X = x;
        tt.Y = y;
    }
}