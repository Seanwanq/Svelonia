using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Svelonia.Core;
using System;
using System.Threading.Tasks;

namespace Svelonia.Fluent;

public enum LiveDragBoundaryMode
{
    Clamp, // 撞墙停下
    Clip   // 自由出入，但视觉裁剪
}

public static class MultiTouchDragExtensions
{
    private class LiveDragState
    {
        public Point StartPointerPosition { get; set; }
        public Point StartPointerPositionOffset { get; set; } 
        public double InitialTransformX { get; set; }
        public double InitialTransformY { get; set; }
        public TranslateTransform Transform { get; set; } = new();
        public bool IsDragging { get; set; }
        public long PointerId { get; set; } = -1;
        public Control? Ghost { get; set; }
        public AdornerLayer? Adorner { get; set; }
    }

    public static T LiveDraggable<T>(this T control, 
        Control? handle = null,
        State<bool>? enable = null, 
        bool animateBack = true, 
        Func<T, Control>? ghostTransform = null,
        Func<Point, Point>? constrain = null,
        Control? constrainTo = null,
        LiveDragBoundaryMode boundaryMode = LiveDragBoundaryMode.Clamp,
        Action? onStart = null,
        Action? onEnd = null,
        Action<Point>? onMove = null) where T : Control
    {
        var state = new LiveDragState();
        var trigger = handle ?? control;

        trigger.PointerPressed += (s, e) =>
        {
            if (enable != null && !enable.Value) return;
            if (state.IsDragging) return;

            var parent = control.Parent as Visual;
            var point = e.GetCurrentPoint(parent);
            
            if (point.Properties.IsLeftButtonPressed)
            {
                EnsureTransform(control, state);
                
                state.Transform.Transitions = null;

                state.IsDragging = true;
                state.PointerId = point.Pointer.Id;
                state.StartPointerPosition = point.Position;
                state.StartPointerPositionOffset = e.GetPosition(control);
                state.InitialTransformX = state.Transform.X;
                state.InitialTransformY = state.Transform.Y;

                state.Adorner = AdornerLayer.GetAdornerLayer(control);

                if (ghostTransform != null && state.Adorner != null)
                {
                    state.Ghost = ghostTransform(control);
                    state.Ghost.IsHitTestVisible = false;
                    state.Adorner.Children.Add(state.Ghost);
                }

                UpdateVisuals(state, control, constrainTo, boundaryMode);
                onStart?.Invoke();

                e.Pointer.Capture(trigger);
                e.Handled = true;
            }
        };

        trigger.PointerMoved += (s, e) =>
        {
            if (!state.IsDragging || e.Pointer.Id != state.PointerId) return;

            var parent = control.Parent as Visual;
            var point = e.GetCurrentPoint(parent);
            var currentPos = point.Position;
            
            var deltaX = currentPos.X - state.StartPointerPosition.X;
            var deltaY = currentPos.Y - state.StartPointerPosition.Y;

            var targetX = state.InitialTransformX + deltaX;
            var targetY = state.InitialTransformY + deltaY;

            if (constrainTo != null && boundaryMode == LiveDragBoundaryMode.Clamp)
            {
                var originInConstrainer = control.TranslatePoint(new Point(0,0), constrainTo);
                if (originInConstrainer.HasValue)
                {
                    var absOriginX = originInConstrainer.Value.X - state.Transform.X;
                    var absOriginY = originInConstrainer.Value.Y - state.Transform.Y;

                    var minX = -absOriginX;
                    var minY = -absOriginY;
                    var maxX = (constrainTo.Bounds.Width - control.Bounds.Width) - absOriginX;
                    var maxY = (constrainTo.Bounds.Height - control.Bounds.Height) - absOriginY;

                    targetX = Math.Clamp(targetX, minX, maxX);
                    targetY = Math.Clamp(targetY, minY, maxY);
                }
            }

            if (constrain != null)
            {
                var constrained = constrain(new Point(targetX, targetY));
                targetX = constrained.X;
                targetY = constrained.Y;
            }

            state.Transform.X = targetX;
            state.Transform.Y = targetY;

            UpdateVisuals(state, control, constrainTo, boundaryMode);
            onMove?.Invoke(new Point(state.Transform.X, state.Transform.Y));
        };

        async void EndDrag(IPointer pointer)
        {
            if (!state.IsDragging) return;
            state.IsDragging = false;
            state.PointerId = -1;
            pointer.Capture(null);

            if (state.Ghost != null && state.Adorner != null)
            {
                state.Adorner.Children.Remove(state.Ghost);
                state.Ghost = null;
            }
            
            onEnd?.Invoke();

            if (animateBack)
            {
                // 监听动画过程中的变化，实时更新裁剪
                EventHandler<AvaloniaPropertyChangedEventArgs> handler = (s, args) => 
                {
                    if (args.Property == TranslateTransform.XProperty || args.Property == TranslateTransform.YProperty)
                    {
                        UpdateVisuals(state, control, constrainTo, boundaryMode);
                    }
                };
                state.Transform.PropertyChanged += handler;

                var duration = TimeSpan.FromMilliseconds(250);
                state.Transform.Transitions = new Transitions
                {
                    new DoubleTransition { Property = TranslateTransform.XProperty, Duration = duration, Easing = new QuadraticEaseOut() },
                    new DoubleTransition { Property = TranslateTransform.YProperty, Duration = duration, Easing = new QuadraticEaseOut() }
                };
                state.Transform.X = 0;
                state.Transform.Y = 0;

                await Task.Delay(duration);
                
                state.Transform.PropertyChanged -= handler;
                state.Transform.Transitions = null;
            }

            // 最终恢复本体裁剪
            control.Clip = null;
        }

        control.PointerReleased += (s, e) => { if(e.Pointer.Id == state.PointerId) EndDrag(e.Pointer); };
        control.PointerCaptureLost += (s, e) => { if(e.Pointer.Id == state.PointerId) EndDrag(e.Pointer); };

        return control;
    }

    private static void UpdateVisuals(LiveDragState state, Control control, Control? constrainTo, LiveDragBoundaryMode mode)
    {
        // 1. 同步 Ghost 位置
        if (state.Ghost != null && state.Adorner != null)
        {
            var ghostPos = control.TranslatePoint(new Point(0, 0), state.Adorner);
            if (ghostPos.HasValue)
            {
                Canvas.SetLeft(state.Ghost, ghostPos.Value.X);
                Canvas.SetTop(state.Ghost, ghostPos.Value.Y);
                
                // 2. 同步 Ghost 裁剪
                if (mode == LiveDragBoundaryMode.Clip && constrainTo != null)
                {
                    var cageInAdorner = constrainTo.TranslatePoint(new Point(0,0), state.Adorner);
                    if (cageInAdorner.HasValue)
                    {
                        state.Ghost.Clip = new RectangleGeometry(new Rect(
                            cageInAdorner.Value.X - ghostPos.Value.X, 
                            cageInAdorner.Value.Y - ghostPos.Value.Y, 
                            constrainTo.Bounds.Width, 
                            constrainTo.Bounds.Height));
                    }
                }
                else state.Ghost.Clip = null;
            }
        }

        // 3. 同步本体 (Control) 裁剪 (修复你发现的问题)
        if (mode == LiveDragBoundaryMode.Clip && constrainTo != null)
        {
            var cageInControl = constrainTo.TranslatePoint(new Point(0, 0), control);
            if (cageInControl.HasValue)
            {
                control.Clip = new RectangleGeometry(new Rect(
                    cageInControl.Value.X, 
                    cageInControl.Value.Y, 
                    constrainTo.Bounds.Width, 
                    constrainTo.Bounds.Height));
            }
        }
        else control.Clip = null;
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
}