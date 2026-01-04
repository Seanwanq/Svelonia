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
    
    /// <summary>
    /// The current view transformation matrix (Scale + Translate).
    /// </summary>
    public State<Matrix> ViewMatrix { get; } = new(Matrix.Identity);

    /// <summary>
    /// The current size of the viewport (Container bounds).
    /// </summary>
    public State<Size> ViewportSize { get; } = new(new Size(0, 0));

    /// <summary>
    /// The minimum allowed zoom scale. Default 0.1.
    /// </summary>
    public double MinZoom { get; set; } = 0.1;

    /// <summary>
    /// The maximum allowed zoom scale. Default 10.0.
    /// </summary>
    public double MaxZoom { get; set; } = 10.0;

    /// <summary>
    /// The zoom step factor. Default 1.05 (5%).
    /// </summary>
    public double ZoomStep { get; set; } = 1.05;

    public InfiniteCanvas()
    {
        // Essential for hit-testing pan events on empty areas
        Background = Brushes.Transparent;
        ClipToBounds = true;
        Focusable = true;
        
        // Ensure content fills the viewport so (0,0) aligns
        HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

        // Track size changes
        this.GetObservable(BoundsProperty).Subscribe(rect => ViewportSize.Value = rect.Size);

        this.OnPointerWheelChanged(OnWheel);
        this.OnPointerPressed(OnPressed);
        this.OnPointerMoved(OnMoved);
        this.OnPointerReleased(OnReleased);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
        {
            if (change.NewValue is Control control)
            {
                // Ensure transform origin is Top-Left to match our Matrix logic
                control.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
                
                // Apply the matrix transform to the content
                control.Bind(Visual.RenderTransformProperty, ViewMatrix.Select(m => new MatrixTransform(m)).ToBinding());
            }
        }
    }

    /// <summary>
    /// Adjusts the view matrix to fit the specified content bounds within the viewport.
    /// </summary>
    public void ZoomToFit(Rect contentBounds, double padding = 50)
    {
        var viewW = Bounds.Width;
        var viewH = Bounds.Height;
        
        if (viewW == 0 || viewH == 0 || contentBounds.Width <= 0 || contentBounds.Height <= 0) return;

        var scaleX = (viewW - 2 * padding) / contentBounds.Width;
        var scaleY = (viewH - 2 * padding) / contentBounds.Height;
        var scale = Math.Min(scaleX, scaleY);
        
        scale = Math.Clamp(scale, MinZoom, MaxZoom);

        var contentCenter = contentBounds.Center;
        var viewCenter = new Point(viewW / 2, viewH / 2);
        
        var mat = Matrix.CreateTranslation(-contentCenter.X, -contentCenter.Y) *
                  Matrix.CreateScale(scale, scale) *
                  Matrix.CreateTranslation(viewCenter.X, viewCenter.Y);
                  
        ViewMatrix.Value = mat;
    }

    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            var p = e.GetPosition(this);
            var delta = e.Delta.Y > 0 ? ZoomStep : (1 / ZoomStep);
            
            var mat = ViewMatrix.Value;
            var currentScale = mat.M11; 

            if (currentScale * delta < MinZoom || currentScale * delta > MaxZoom) return;

            var transform = Matrix.CreateTranslation(-p.X, -p.Y) 
                          * Matrix.CreateScale(delta, delta) 
                          * Matrix.CreateTranslation(p.X, p.Y);
                              
            ViewMatrix.Value = mat * transform;
        }
        else
        {
            // Support Touchpad two-finger panning / Mouse wheel panning
            e.Handled = true;
            
            // High sensitivity and diagonal boosting to counteract OS-level axis snapping.
            var sensitivity = 45.0;
            var dx = e.Delta.X;
            var dy = e.Delta.Y;

            // If we have movement in both axes, slightly boost the vector to help break 
            // the system's "sticky" horizontal/vertical lock.
            if (dx != 0 && dy != 0)
            {
                var ratio = Math.Abs(dx / dy);
                if (ratio > 0.15 && ratio < 6.0) 
                {
                    dx *= 1.2;
                    dy *= 1.2;
                }
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
        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
        }
    }
}