using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Diagnostics;
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
    
    // Animation
    private readonly DispatcherTimer _animationTimer;
    private Matrix _targetMatrix;
    private bool _isAnimating;

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

        _animationTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Input, OnAnimationTick);
        _animationTimer.Stop();

        this.GetObservable(BoundsProperty).Subscribe(rect => ViewportSize.Value = rect.Size);

        this.OnPointerWheelChanged(OnWheel);
        this.OnPointerPressed(OnPressed);
        this.OnPointerMoved(OnMoved);
        this.OnPointerReleased(OnReleased);
    }

    private void CancelAnimation()
    {
        if (_isAnimating)
        {
            _isAnimating = false;
            _animationTimer.Stop();
        }
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (!_isAnimating)
        {
            _animationTimer.Stop();
            return;
        }

        var current = ViewMatrix.Value;
        var target = _targetMatrix;

        // Simple Lerp for Translation (assuming Scale is constant during this anim)
        // Lerp factor 0.2 for smooth spring-like ease-out
        double lerp = 0.2;

        double nextM31 = current.M31 + (target.M31 - current.M31) * lerp;
        double nextM32 = current.M32 + (target.M32 - current.M32) * lerp;

        // Check if close enough
        if (Math.Abs(target.M31 - nextM31) < 0.5 && Math.Abs(target.M32 - nextM32) < 0.5)
        {
            ViewMatrix.Value = target;
            _isAnimating = false;
            _animationTimer.Stop();
        }
        else
        {
            // Reconstruct matrix with new translation (preserving scale/rotation from current)
            // Note: This assumes only translation changes. If scale changes, we need to lerp M11/M22 too.
            // For EnsureVisible, it's only translation.
            
            // Safer: Matrix copy
            var next = new Matrix(
                current.M11, current.M12,
                current.M21, current.M22,
                nextM31, nextM32);
            
            ViewMatrix.Value = next;
        }
    }

    private void AnimateTo(Matrix target)
    {
        _targetMatrix = target;
        if (!_isAnimating)
        {
            _isAnimating = true;
            _animationTimer.Start();
        }
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
        EnsureVisible(worldRect, new Thickness(padding), instant);
    }

    /// <summary>
    /// Smoothly adjusts the view to keep the target rectangle visible with asymmetric padding.
    /// </summary>
    public void EnsureVisible(Rect worldRect, Thickness padding, bool instant = false)
    {
        if (_isPanning) return;

        var mat = ViewMatrix.Value;
        var size = ViewportSize.Value;
        if (size.Width <= 0 || size.Height <= 0) return;

        // Adaptive padding: don't let padding consume the entire screen
        double safeL = Math.Min(padding.Left, size.Width * 0.4);
        double safeR = Math.Min(padding.Right, size.Width * 0.4);
        double safeT = Math.Min(padding.Top, size.Height * 0.4);
        double safeB = Math.Min(padding.Bottom, size.Height * 0.4);

        var viewRect = worldRect.TransformToAABB(mat);
        var viewport = new Rect(safeL, safeT, size.Width - (safeL + safeR), size.Height - (safeT + safeB));

        if (viewport.Contains(viewRect)) 
        {
            return;
        }

        double dx = 0;
        double dy = 0;

        if (viewRect.Left < viewport.Left) dx = viewport.Left - viewRect.Left;
        else if (viewRect.Right > viewport.Right) dx = viewport.Right - viewRect.Right;

        if (viewRect.Top < viewport.Top) dy = viewport.Top - viewRect.Top;
        else if (viewRect.Bottom > viewport.Bottom) dy = viewport.Bottom - viewRect.Bottom;

        if (dx == 0 && dy == 0) return;

        var targetMatrix = mat * Matrix.CreateTranslation(dx, dy);

        if (instant)
        {
            CancelAnimation();
            ViewMatrix.Value = targetMatrix;
        }
        else
        {
            AnimateTo(targetMatrix);
        }
    }

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        CancelAnimation();
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
        CancelAnimation();
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
