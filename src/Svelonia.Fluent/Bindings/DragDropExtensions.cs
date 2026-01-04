using System;
using System.Collections;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace Svelonia.Fluent;

public enum DragVisualMode
{
    None,
    Ghost,
    Move,
}

public static class DragDropExtensions
{
    private static Control? _lastDropTarget;
    private static Point? _lastGlobalDropPos;

    private class DragState
    {
        public Point StartPoint { get; set; }
        public Point InitialMouseOffset { get; set; }
        public bool IsPressed { get; set; }
    }

    public static T Draggable<T>(
        this T control,
        object data,
        DragDropEffects effect = DragDropEffects.Copy,
        Control? handle = null,
        string? format = null,
        Svelonia.Core.State<bool>? enable = null,
        DragVisualMode visualMode = DragVisualMode.Ghost,
        Func<Image, Control>? ghostTransform = null,
        bool animateBack = true,
        Action? onStart = null,
        Action<Point>? onDrag = null,
        Action<DragDropEffects>? onEnd = null
    )
        where T : Control
    {
        var trigger = handle ?? control;
        var state = new DragState();

        trigger.PointerPressed += (s, e) =>
        {
            if (enable != null && !enable.Value)
                return;
            if (e.GetCurrentPoint(trigger).Properties.IsLeftButtonPressed)
            {
                // RESET GLOBAL STATE AT START OF INTERACTION
                _lastDropTarget = null;
                _lastGlobalDropPos = null;

                state.StartPoint = e.GetPosition(trigger);
                state.InitialMouseOffset = e.GetPosition(control);
                state.IsPressed = true;
                e.Handled = false;
            }
        };

        trigger.PointerReleased += (s, e) =>
        {
            state.IsPressed = false;
        };

        trigger.PointerMoved += async (s, e) =>
        {
            if (!state.IsPressed)
                return;

            var currentPoint = e.GetPosition(trigger);
            var distance = Math.Sqrt(
                Math.Pow(currentPoint.X - state.StartPoint.X, 2)
                    + Math.Pow(currentPoint.Y - state.StartPoint.Y, 2)
            );

            if (distance > 10)
            {
                state.IsPressed = false;
                var dataObject = CreateDataObject(data, format);
                onStart?.Invoke();

                var originalOpacity = control.Opacity;
                var originalAllowDrop = control.GetValue(DragDrop.AllowDropProperty);
                object? triggerOriginalAllowDrop =
                    (trigger != control) ? trigger.GetValue(DragDrop.AllowDropProperty) : null;

                control.SetValue(DragDrop.AllowDropProperty, true);
                if (trigger != control)
                    trigger.SetValue(DragDrop.AllowDropProperty, true);

                Control? ghostContainer = null;
                AdornerLayer? adornerLayer = null;
                EventHandler<DragEventArgs>? dragOverHandler = null;
                EventHandler<DragEventArgs>? catchAllHandler = null;
                TopLevel? topLevel = TopLevel.GetTopLevel(control);
                object? topLevelOriginalAllowDrop = null;
                Point lastGhostPos = default;

                if (visualMode != DragVisualMode.None && topLevel != null)
                {
                    topLevelOriginalAllowDrop = topLevel.GetValue(DragDrop.AllowDropProperty);
                    topLevel.SetValue(DragDrop.AllowDropProperty, true);

                    try
                    {
                        adornerLayer = AdornerLayer.GetAdornerLayer(control);
                        if (adornerLayer != null)
                        {
                            var bitmap = new RenderTargetBitmap(
                                new PixelSize(
                                    (int)control.Bounds.Width,
                                    (int)control.Bounds.Height
                                ),
                                new Vector(96, 96)
                            );
                            bitmap.Render(control);

                            var ghostImage = new Image
                            {
                                Source = bitmap,
                                Width = control.Bounds.Width,
                                Height = control.Bounds.Height,
                                Opacity = (visualMode == DragVisualMode.Move) ? 1.0 : 0.7,
                                IsHitTestVisible = false,
                            };

                            ghostContainer =
                                ghostTransform != null ? ghostTransform(ghostImage) : ghostImage;
                            ghostContainer.IsHitTestVisible = false;

                            if (onStart == null)
                                control.Opacity = (visualMode == DragVisualMode.Move) ? 0.0 : 0.5;

                            var currentPos = e.GetPosition(adornerLayer);
                            lastGhostPos = new Point(
                                currentPos.X - state.InitialMouseOffset.X,
                                currentPos.Y - state.InitialMouseOffset.Y
                            );
                            Canvas.SetLeft(ghostContainer, lastGhostPos.X);
                            Canvas.SetTop(ghostContainer, lastGhostPos.Y);
                            adornerLayer.Children.Add(ghostContainer);

                            dragOverHandler = (sender, args) =>
                            {
                                var pos = args.GetPosition(adornerLayer);
                                lastGhostPos = new Point(
                                    pos.X - state.InitialMouseOffset.X,
                                    pos.Y - state.InitialMouseOffset.Y
                                );
                                Canvas.SetLeft(ghostContainer, lastGhostPos.X);
                                Canvas.SetTop(ghostContainer, lastGhostPos.Y);
                                onDrag?.Invoke(args.GetPosition(topLevel));
                            };
                            topLevel.AddHandler(
                                DragDrop.DragOverEvent,
                                dragOverHandler,
                                RoutingStrategies.Bubble,
                                handledEventsToo: true
                            );

                            // Keep drag alive but DON'T mark as success automatically
                            catchAllHandler = (sender, args) =>
                            {
                                if (!args.Handled)
                                {
                                    args.DragEffects = effect;
                                }
                            };
                            topLevel.AddHandler(
                                DragDrop.DragOverEvent,
                                catchAllHandler,
                                RoutingStrategies.Bubble
                            );
                        }
                    }
                    catch { }
                }

                DragDropEffects result = DragDropEffects.None;
                try
                {
#pragma warning disable CS0618
                    result = await DragDrop.DoDragDrop(e, dataObject, effect);
#pragma warning restore CS0618
                }
                finally
                {
                    // STRICT VALIDATION: Success only if result matches AND a recognized target was hit
                    bool isRealSuccess =
                        (result != DragDropEffects.None) && (_lastDropTarget != null);
                    Console.WriteLine(
                        $"[Drag] Finished. OS Result: {result}, Target Hit: {_lastDropTarget?.GetType().Name ?? "None"}, Final Status: {(isRealSuccess ? "SUCCESS" : "RETURN")}"
                    );

                    if (
                        animateBack
                        && ghostContainer != null
                        && adornerLayer != null
                        && topLevel != null
                    )
                    {
                        Point targetPos;
                        if (!isRealSuccess)
                        {
                            targetPos =
                                control.TranslatePoint(new Point(0, 0), adornerLayer)
                                ?? new Point(0, 0);
                        }
                        else
                        {
                            // Precision Centered Landing
                            var targetBounds = _lastDropTarget!.Bounds;
                            var targetCenterLocal = new Point(
                                targetBounds.Width / 2,
                                targetBounds.Height / 2
                            );
                            var targetCenterGlobal =
                                _lastDropTarget.TranslatePoint(targetCenterLocal, adornerLayer)
                                ?? new Point(0, 0);
                            targetPos = new Point(
                                targetCenterGlobal.X - control.Bounds.Width / 2,
                                targetCenterGlobal.Y - control.Bounds.Height / 2
                            );
                        }

                        await AnimateGhost(ghostContainer, targetPos, fadeOut: isRealSuccess);
                    }

                    // Restore Source Opacity ONLY if we are NOT successfully moving away
                    if (!isRealSuccess)
                        control.Opacity = originalOpacity;

                    control.SetValue(DragDrop.AllowDropProperty, originalAllowDrop);
                    if (triggerOriginalAllowDrop != null && trigger != control)
                        trigger.SetValue(DragDrop.AllowDropProperty, triggerOriginalAllowDrop);

                    if (topLevel != null && topLevelOriginalAllowDrop != null)
                        topLevel.SetValue(DragDrop.AllowDropProperty, topLevelOriginalAllowDrop);

                    if (ghostContainer != null && adornerLayer != null)
                        adornerLayer.Children.Remove(ghostContainer);
                    if (dragOverHandler != null && topLevel != null)
                        topLevel.RemoveHandler(DragDrop.DragOverEvent, dragOverHandler);
                    if (catchAllHandler != null && topLevel != null)
                        topLevel.RemoveHandler(DragDrop.DragOverEvent, catchAllHandler);

                    onEnd?.Invoke(isRealSuccess ? result : DragDropEffects.None);
                }
            }
        };

        return control;
    }

    private static async Task AnimateGhost(Control ghost, Point end, bool fadeOut = false)
    {
        var duration = TimeSpan.FromMilliseconds(200);
        var transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = Canvas.LeftProperty,
                Duration = duration,
                Easing = new QuadraticEaseOut(),
            },
            new DoubleTransition
            {
                Property = Canvas.TopProperty,
                Duration = duration,
                Easing = new QuadraticEaseOut(),
            },
        };
        if (fadeOut)
            transitions.Add(
                new DoubleTransition
                {
                    Property = Visual.OpacityProperty,
                    Duration = duration,
                    Easing = new QuadraticEaseOut(),
                }
            );

        ghost.Transitions = transitions;
        Canvas.SetLeft(ghost, end.X);
        Canvas.SetTop(ghost, end.Y);
        if (fadeOut)
            ghost.Opacity = 0;
        await Task.Delay(duration);
    }

    public static T OnDrop<T>(this T control, Action<DragEventArgs> handler)
        where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(
            DragDrop.DropEvent,
            (s, e) =>
            {
                _lastDropTarget = control;
                handler(e);
            }
        );
        return control;
    }

    public static T OnDragOver<T>(this T control, Action<DragEventArgs> handler)
        where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragOverEvent, (s, e) => handler(e));
        return control;
    }

    public static T OnDragEnter<T>(this T control, Action<DragEventArgs> handler)
        where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragEnterEvent, (s, e) => handler(e));
        return control;
    }

    public static T OnDragLeave<T>(this T control, Action<RoutedEventArgs> handler)
        where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragLeaveEvent, (s, e) => handler(e));
        return control;
    }

    private static DataObject CreateDataObject(object data, string? format)
    {
#pragma warning disable CS0618
        var dataObject = new DataObject();
        if (!string.IsNullOrEmpty(format))
            dataObject.Set(format, data);
        else if (data is string str)
            dataObject.Set("Text", str);
        else if (data is IEnumerable<string> files)
            dataObject.Set("FileNames", files);
        else
        {
            dataObject.Set("SveloniaData", data);
            dataObject.Set("Text", data.ToString() ?? "");
        }
        return dataObject;
#pragma warning restore CS0618
    }
}
