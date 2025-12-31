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
    /// <param name="control">The control to be dragged (or the payload source).</param>
    /// <param name="data">The data to transfer. Can be a string (Text), IEnumerable&lt;string&gt; (Files), or any object.</param>
    /// <param name="effect">The allowed drag effects (Copy, Move, Link).</param>
    /// <param name="handle">Optional. A child control that acts as the drag handle. If null, the entire control is the handle.</param>
    /// <param name="format">Optional. Custom data format string.</param>
    /// <param name="enable">Optional. State to control whether dragging is enabled.</param>
    /// <param name="visualMode">Optional. Visual feedback style. Default is Ghost.</param>
    /// <param name="onStart">Optional. Callback when drag starts.</param>
    /// <param name="onEnd">Optional. Callback when drag ends.</param>
    public static T Draggable<T>(this T control, 
        object data, 
        DragDropEffects effect = DragDropEffects.Copy, 
        Control? handle = null, 
        string? format = null,
        Svelonia.Core.State<bool>? enable = null,
        DragVisualMode visualMode = DragVisualMode.Ghost,
        Action? onStart = null,
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

        trigger.PointerReleased += (s, e) =>
        {
            state.IsPressed = false;
        };

        trigger.PointerMoved += async (s, e) =>
        {
            if (!state.IsPressed) return;

            var currentPoint = e.GetPosition(trigger);
            var distance = Math.Sqrt(Math.Pow(currentPoint.X - state.StartPoint.X, 2) + Math.Pow(currentPoint.Y - state.StartPoint.Y, 2));

            // Threshold for drag start (e.g. 10 pixels)
            if (distance > 10)
            {
                state.IsPressed = false; // Stop tracking once drag starts

                var dataObject = CreateDataObject(data, format);
                
                // Fire drag operation
                onStart?.Invoke();
                
                // Force AllowDrop to ensure DragOver fires on self/handle (prevents ghost freeze on self)
                control.SetValue(DragDrop.AllowDropProperty, true);
                if (trigger != control) trigger.SetValue(DragDrop.AllowDropProperty, true);

                // Ghost Setup vars
                Control? ghost = null;
                AdornerLayer? adornerLayer = null;
                EventHandler<DragEventArgs>? dragOverHandler = null;
                EventHandler<DragEventArgs>? catchAllHandler = null;
                TopLevel? topLevel = TopLevel.GetTopLevel(control);
                object? topLevelOriginalAllowDrop = null;

                // Visual Feedback state
                var originalOpacity = control.Opacity;
                var originalAllowDrop = control.GetValue(DragDrop.AllowDropProperty); 
                object? triggerOriginalAllowDrop = (trigger != control) ? trigger.GetValue(DragDrop.AllowDropProperty) : null;

                bool useGhost = visualMode != DragVisualMode.None;

                if (useGhost && topLevel != null)
                {
                    // Ensure TopLevel receives DragOver events everywhere
                    topLevelOriginalAllowDrop = topLevel.GetValue(DragDrop.AllowDropProperty);
                    topLevel.SetValue(DragDrop.AllowDropProperty, true);

                    try 
                    {
                        adornerLayer = AdornerLayer.GetAdornerLayer(control);
                        if (adornerLayer != null)
                        {
                            // Capture bitmap BEFORE modifying opacity
                            var bitmap = new RenderTargetBitmap(new PixelSize((int)control.Bounds.Width, (int)control.Bounds.Height), new Vector(96, 96));
                            bitmap.Render(control);

                            ghost = new Image 
                            { 
                                Source = bitmap, 
                                Width = control.Bounds.Width, 
                                Height = control.Bounds.Height,
                                Opacity = visualMode == DragVisualMode.Move ? 1.0 : 0.7,
                                IsHitTestVisible = false
                            };

                            // Apply Visual Feedback AFTER capture
                            if (onStart == null)
                            {
                                if (visualMode == DragVisualMode.Ghost) control.Opacity = 0.5;
                                else if (visualMode == DragVisualMode.Move) control.Opacity = 0.0;
                            }

                            // Initial Position (relative to AdornerLayer)
                            // We use the current pointer position to place it immediately
                            var currentPos = e.GetPosition(adornerLayer);
                            Canvas.SetLeft(ghost, currentPos.X - ghost.Width / 2);
                            Canvas.SetTop(ghost, currentPos.Y - ghost.Height / 2);

                            adornerLayer.Children.Add(ghost);

                            // Track Mouse (Tunneling) - Updates Ghost Position
                            dragOverHandler = (sender, args) =>
                            {
                                var pos = args.GetPosition(adornerLayer);
                                Canvas.SetLeft(ghost, pos.X - ghost.Width / 2);
                                Canvas.SetTop(ghost, pos.Y - ghost.Height / 2);
                            };
                            // DragOver is Bubble-only in Avalonia, so Tunnel won't work.
                            // We use Bubble with handledEventsToo: true to capture it at TopLevel even if children handled it.
                            topLevel.AddHandler(DragDrop.DragOverEvent, dragOverHandler, RoutingStrategies.Bubble, handledEventsToo: true);

                            // Catch-all (Bubbling) - Ensures DragOver keeps firing over inert controls
                            catchAllHandler = (sender, args) =>
                            {
                                if (!args.Handled)
                                {
                                    args.DragEffects = effect;
                                    args.Handled = true;
                                }
                            };
                            topLevel.AddHandler(DragDrop.DragOverEvent, catchAllHandler, RoutingStrategies.Bubble);
                        }
                    }
                    catch
                    {
                        // Fallback if ghost creation fails
                        if (ghost != null && adornerLayer != null) adornerLayer.Children.Remove(ghost);
                        ghost = null;
                    }
                }

                try
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    await DragDrop.DoDragDrop(e, dataObject, effect);
#pragma warning restore CS0618 // Type or member is obsolete
                }
                finally
                {
                    if (onStart == null) control.Opacity = originalOpacity;
                    control.SetValue(DragDrop.AllowDropProperty, originalAllowDrop);
                    if (triggerOriginalAllowDrop != null && trigger != control) trigger.SetValue(DragDrop.AllowDropProperty, triggerOriginalAllowDrop);
                    
                    if (topLevel != null && topLevelOriginalAllowDrop != null)
                    {
                        topLevel.SetValue(DragDrop.AllowDropProperty, topLevelOriginalAllowDrop);
                    }

                    // Cleanup Ghost
                    if (ghost != null && adornerLayer != null)
                    {
                        adornerLayer.Children.Remove(ghost);
                    }
                    if (dragOverHandler != null && topLevel != null)
                    {
                        topLevel.RemoveHandler(DragDrop.DragOverEvent, dragOverHandler);
                    }
                    if (catchAllHandler != null && topLevel != null)
                    {
                        topLevel.RemoveHandler(DragDrop.DragOverEvent, catchAllHandler);
                    }

                    onEnd?.Invoke();
                }
            }
        };

        return control;
    }

    /// <summary>
    /// Sets the control as a Drop Target.
    /// </summary>
    public static T OnDrop<T>(this T control, Action<DragEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DropEvent, (s, e) => handler(e));
        return control;
    }

    /// <summary>
    /// Handle DragOver event (e.g. to validate data or change cursor).
    /// </summary>
    public static T OnDragOver<T>(this T control, Action<DragEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragOverEvent, (s, e) => handler(e));
        return control;
    }

    /// <summary>
    /// Handle DragEnter event (e.g. for visual feedback).
    /// </summary>
    public static T OnDragEnter<T>(this T control, Action<DragEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragEnterEvent, (s, e) => handler(e));
        return control;
    }

    /// <summary>
    /// Handle DragLeave event (e.g. to clean up visual feedback).
    /// </summary>
    public static T OnDragLeave<T>(this T control, Action<RoutedEventArgs> handler) where T : Control
    {
        DragDrop.SetAllowDrop(control, true);
        control.AddHandler(DragDrop.DragLeaveEvent, (s, e) => handler(e));
        return control;
    }

    private static DataObject CreateDataObject(object data, string? format)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var dataObject = new DataObject();
        
        if (!string.IsNullOrEmpty(format))
        {
            dataObject.Set(format, data);
            return dataObject;
        }

        if (data is string str)
        {
            dataObject.Set("Text", str);
        }
        else if (data is IEnumerable<string> files)
        {
             // Simple heuristic check if it looks like files
             dataObject.Set("FileNames", files);
        }
        else
        {
            // Default object fallback
            dataObject.Set("SveloniaData", data);
            
            // Also try to set ToString as text for external compatibility
            dataObject.Set("Text", data.ToString() ?? "");
        }

        return dataObject;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}