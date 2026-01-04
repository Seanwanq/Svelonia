using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Svelonia.Core;
using Svelonia.Fluent;

namespace Svelonia.Controls;

/// <summary>
/// A high-performance infinite canvas that supports Pan and Zoom via Matrix RenderTransform.
/// </summary>
public class InfiniteCanvas : ContentControl
{
    private bool _isPanning;
    private Point _lastMousePos;
    
    public State<Matrix> ViewMatrix { get; } = new(Matrix.Identity);
    public State<Size> ViewportSize { get; } = new(new Size(0, 0));

    public double MinZoom { get; set; } = 0.1;
    public double MaxZoom { get; set; } = 10.0;
    public double ZoomStep { get; set; } = 1.05;

    public InfiniteCanvas()
    {
        Background = Brushes.Transparent;
        ClipToBounds = true;
        Focusable = true;
        HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

        this.GetObservable(BoundsProperty).Subscribe(rect => ViewportSize.Value = rect.Size);

        this.OnPointerWheelChanged(OnWheel);
        this.OnPointerPressed(OnPressed);
        this.OnPointerMoved(OnMoved);
        this.OnPointerReleased(OnReleased);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty && change.NewValue is Control control)
        {
            control.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
            control.Bind(Visual.RenderTransformProperty, ViewMatrix.Select(m => new MatrixTransform(m)).ToBinding());
        }
    }

    public void ZoomToFit(Rect contentBounds, double padding = 50)
    {
        var viewW = Bounds.Width;
        var viewH = Bounds.Height;
        if (viewW <= 0 || viewH <= 0 || contentBounds.Width <= 0 || contentBounds.Height <= 0) return;

        var scaleX = (viewW - 2 * padding) / contentBounds.Width;
        var scaleY = (viewH - 2 * padding) / contentBounds.Height;
        var scale = Math.Clamp(Math.Min(scaleX, scaleY), MinZoom, MaxZoom);

        var contentCenter = contentBounds.Center;
        var viewCenter = new Point(viewW / 2, viewH / 2);
        
        ViewMatrix.Value = Matrix.CreateTranslation(-contentCenter.X, -contentCenter.Y) *
                          Matrix.CreateScale(scale, scale) *
                          Matrix.CreateTranslation(viewCenter.X, viewCenter.Y);
    }

    /// <summary>
    /// Smoothly adjusts the view to keep the target rectangle visible.
    /// </summary>
    public void EnsureVisible(Rect worldRect, double padding = 100, bool instant = false)
    {
        var mat = ViewMatrix.Value;
        var size = ViewportSize.Value;
        if (size.Width <= 0 || size.Height <= 0) return;

        var viewRect = worldRect.TransformToAABB(mat);
        var viewport = new Rect(padding, padding, size.Width - padding * 2, size.Height - padding * 2);

        if (viewport.Contains(viewRect)) return;

        double dx = 0;
        double dy = 0;

        if (viewRect.Left < viewport.Left) dx = viewport.Left - viewRect.Left;
        else if (viewRect.Right > viewport.Right) dx = viewport.Right - viewRect.Right;

        if (viewRect.Top < viewport.Top) dy = viewport.Top - viewRect.Top;
        else if (viewRect.Bottom > viewport.Bottom) dy = viewport.Bottom - viewRect.Bottom;

        if (dx == 0 && dy == 0) return;

        // DEBUG LOG
        // Console.WriteLine($"[InfiniteCanvas] EnsureVisible: dx={dx:F1}, dy={dy:F1} | Instant={instant}");

        if (instant)
        {
            ViewMatrix.Value = mat * Matrix.CreateTranslation(dx, dy);
        }
        else
        {
            ViewMatrix.Value = mat * Matrix.CreateTranslation(dx * 0.3, dy * 0.3);
        }
    }

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            var p = e.GetPosition(this);
            var delta = e.Delta.Y > 0 ? ZoomStep : (1 / ZoomStep);
            var mat = ViewMatrix.Value;
            if (mat.M11 * delta < MinZoom || mat.M11 * delta > MaxZoom) return;

            ViewMatrix.Value = mat * Matrix.CreateTranslation(-p.X, -p.Y) 
                                   * Matrix.CreateScale(delta, delta) 
                                   * Matrix.CreateTranslation(p.X, p.Y);
        }
        else
        {
            e.Handled = true;
            var sensitivity = 45.0;
            var dx = e.Delta.X;
            var dy = e.Delta.Y;
            if (dx != 0 && dy != 0)
            {
                var ratio = Math.Abs(dx / dy);
                if (ratio > 0.15 && ratio < 6.0) { dx *= 1.2; dy *= 1.2; }
            }
            ViewMatrix.Value = ViewMatrix.Value * Matrix.CreateTranslation(dx * sensitivity, dy * sensitivity);
        }
    }

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsRightButtonPressed || props.IsMiddleButtonPressed || e.Pointer.Type == PointerType.Touch)
        {
            _isPanning = true;
            _lastMousePos = e.GetPosition(this);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (_isPanning)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastMousePos;
            _lastMousePos = pos;
            ViewMatrix.Value = ViewMatrix.Value * Matrix.CreateTranslation(delta.X, delta.Y);
        }
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning) { _isPanning = false; e.Pointer.Capture(null); }
    }
}
