using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using System.Collections;

namespace Svelonia.Fluent;

public enum DragVisualMode
{
    None,       // No visual ghost
    Ghost,      // Semi-transparent ghost (70%), source dimmed (50%)
    Move        // Opaque ghost (100%), source hidden (0%) - simulates moving the object itself
}

public static class DragDropExtensions
{
    private class DragState
    {
        public Point StartPoint { get; set; }
        public bool IsPressed { get; set; }
    }

    /// <summary>
    /// Enables Drag and Drop support for the control.
    /// </summary>
    public static T Draggable<T>(this T control, 
        object data, 
        DragDropEffects effect = DragDropEffects.Copy, 
        Control? handle = null, 
        string? format = null,
        Svelonia.Core.State<bool>? enable = null,
        DragVisualMode visualMode = DragVisualMode.Ghost,
        Func<Image, Control>? ghostTransform = null,
        Action? onStart = null,
        Action<Point>? onDrag = null,
        Action? onEnd = null) 
        where T : Control
    {
        var trigger = handle ?? control;
        var state = new DragState();

        trigger.PointerPressed += (s, e) =>
        {
            if (enable != null && !enable.Value) return;
            if (e.GetCurrentPoint(trigger).Properties.IsLeftButtonPressed)
            {
                state.StartPoint = e.GetPosition(trigger);
                state.IsPressed = true;
                e.Handled = false; 
            }
        };

        trigger.PointerReleased += (s, e) => { state.IsPressed = false; };

        trigger.PointerMoved += async (s, e) =>
        {
            if (!state.IsPressed) return;

            var currentPoint = e.GetPosition(trigger);
            var distance = Math.Sqrt(Math.Pow(currentPoint.X - state.StartPoint.X, 2) + Math.Pow(currentPoint.Y - state.StartPoint.Y, 2));

            if (distance > 10)
            {
                state.IsPressed = false;
                var dataObject = CreateDataObject(data, format);
                // Fire drag operation
                onStart?.Invoke();
                
                // Ensure UI is updated before capture
                control.UpdateLayout();

                // Force AllowDrop to ensure DragOver fires on self/handle (prevents ghost freeze on self)
                var originalOpacity = control.Opacity;
                var originalAllowDrop = control.GetValue(DragDrop.AllowDropProperty);
                object? triggerOriginalAllowDrop = (trigger != control) ? trigger.GetValue(DragDrop.AllowDropProperty) : null;

                // Force AllowDrop for continuous events
                control.SetValue(DragDrop.AllowDropProperty, true);
                if (trigger != control) trigger.SetValue(DragDrop.AllowDropProperty, true);

                Control? ghostContainer = null;
                AdornerLayer? adornerLayer = null;
                EventHandler<DragEventArgs>? dragOverHandler = null;
                EventHandler<DragEventArgs>? catchAllHandler = null;
                TopLevel? topLevel = TopLevel.GetTopLevel(control);
                object? topLevelOriginalAllowDrop = null;

                if (visualMode != DragVisualMode.None && topLevel != null)
                {
                    topLevelOriginalAllowDrop = topLevel.GetValue(DragDrop.AllowDropProperty);
                    topLevel.SetValue(DragDrop.AllowDropProperty, true);

                    try 
                    {
                        adornerLayer = AdornerLayer.GetAdornerLayer(control);
                        if (adornerLayer != null)
                        {
                            // 1. Capture snapshot BEFORE hiding source
                            var bitmap = new RenderTargetBitmap(new PixelSize((int)control.Bounds.Width, (int)control.Bounds.Height), new Vector(96, 96));
                            bitmap.Render(control);

                            var ghostImage = new Image 
                            { 
                                Source = bitmap, 
                                Width = control.Bounds.Width, 
                                Height = control.Bounds.Height,
                                Opacity = (visualMode == DragVisualMode.Move) ? 1.0 : 0.7,
                                IsHitTestVisible = false
                            };

                            // 2. Wrap/Transform Ghost
                            ghostContainer = ghostTransform != null ? ghostTransform(ghostImage) : ghostImage;
                            ghostContainer.IsHitTestVisible = false;

                            // 3. Hide/Dim source AFTER capture
                            if (onStart == null)
                            {
                                control.Opacity = (visualMode == DragVisualMode.Move) ? 0.0 : 0.5;
                            }

                            // 4. Initial positioning
                            var currentPos = e.GetPosition(adornerLayer);
                            Canvas.SetLeft(ghostContainer, currentPos.X - control.Bounds.Width / 2);
                            Canvas.SetTop(ghostContainer, currentPos.Y - control.Bounds.Height / 2);
                            adornerLayer.Children.Add(ghostContainer);

                            // 5. Update loop
                            dragOverHandler = (sender, args) =>
                            {
                                var pos = args.GetPosition(adornerLayer);
                                Canvas.SetLeft(ghostContainer, pos.X - control.Bounds.Width / 2);
                                Canvas.SetTop(ghostContainer, pos.Y - control.Bounds.Height / 2);
                                onDrag?.Invoke(args.GetPosition(topLevel));
                            };
                            topLevel.AddHandler(DragDrop.DragOverEvent, dragOverHandler, RoutingStrategies.Bubble, handledEventsToo: true);

                            catchAllHandler = (sender, args) =>
                            {
                                if (!args.Handled) { args.DragEffects = effect; args.Handled = true; }
                            };
                            topLevel.AddHandler(DragDrop.DragOverEvent, catchAllHandler, RoutingStrategies.Bubble);
                        }
                    }
                    catch { /* Fallback */ }
                }

                try
                {
#pragma warning disable CS0618
                    await DragDrop.DoDragDrop(e, dataObject, effect);
#pragma warning restore CS0618
                }
                finally
                {
                    // Cleanup
                    control.Opacity = originalOpacity;
                    control.SetValue(DragDrop.AllowDropProperty, originalAllowDrop);
                    if (triggerOriginalAllowDrop != null && trigger != control) trigger.SetValue(DragDrop.AllowDropProperty, triggerOriginalAllowDrop);
                    
                    if (topLevel != null && topLevelOriginalAllowDrop != null)
                        topLevel.SetValue(DragDrop.AllowDropProperty, topLevelOriginalAllowDrop);

                    if (ghostContainer != null && adornerLayer != null) adornerLayer.Children.Remove(ghostContainer);
                    if (dragOverHandler != null && topLevel != null) topLevel.RemoveHandler(DragDrop.DragOverEvent, dragOverHandler);
                    if (catchAllHandler != null && topLevel != null) topLevel.RemoveHandler(DragDrop.DragOverEvent, catchAllHandler);

                    onEnd?.Invoke();
                }
            }
        };

        return control;
    }

    public static T OnDrop<T>(this T control, Action<DragEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DropEvent, (s, e) => handler(e));
        return control;
    }

    public static T OnDragOver<T>(this T control, Action<DragEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragOverEvent, (s, e) => handler(e));
        return control;
    }

    public static T OnDragEnter<T>(this T control, Action<DragEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragEnterEvent, (s, e) => handler(e));
        return control;
    }

    public static T OnDragLeave<T>(this T control, Action<RoutedEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragLeaveEvent, (s, e) => handler(e));
        return control;
    }

    private static DataObject CreateDataObject(object data, string? format)
    {
#pragma warning disable CS0618
        var dataObject = new DataObject();
        if (!string.IsNullOrEmpty(format)) dataObject.Set(format, data);
        else if (data is string str) dataObject.Set("Text", str);
        else if (data is IEnumerable<string> files) dataObject.Set("FileNames", files);
        else { dataObject.Set("SveloniaData", data); dataObject.Set("Text", data.ToString() ?? ""); }
        return dataObject;
#pragma warning restore CS0618
    }
}
