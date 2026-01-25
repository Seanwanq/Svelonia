using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Svelonia.Core;

namespace Svelonia.Controls;

/// <summary>
/// Utility to handle high-performance reactive culling (virtualization) for InfiniteCanvas.
/// </summary>
public static class ReactiveViewport
{
    /// <summary>
    /// Creates a Computed IEnumerable that filters items based on their visibility in the InfiniteCanvas.
    /// Includes built-in throttling to avoid UI churn during smooth movements.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="canvas">The infinite canvas to track.</param>
    /// <param name="source">The reactive source of all items.</param>
    /// <param name="isVisible">Predicate to determine if an item is visible within the given world rectangle.</param>
    /// <param name="moveThreshold">Minimum movement in pixels to trigger a re-filter. Default 50.</param>
    /// <param name="scaleThreshold">Minimum scale change (0.0 to 1.0) to trigger a re-filter. Default 0.1 (10%).</param>
    /// <param name="buffer">Padding added to the viewport rectangle in world coordinates. Default 1000.</param>
    /// <param name="onVisibilityChanged">Optional callback to receive (entered, exited) items. Useful for pausing/resuming physics.</param>
    public static Computed<IEnumerable<T>> CreateVisibleSet<T>(
        InfiniteCanvas canvas,
        State<IEnumerable<T>> source,
        Func<Rect, T, bool> isVisible,
        double moveThreshold = 50,
        double scaleThreshold = 0.1,
        double buffer = 1000,
        Action<IEnumerable<T>, IEnumerable<T>>? onVisibilityChanged = null)
    {
        // Internal cache state
        Matrix lastMat = Matrix.Identity;
        IEnumerable<T>? lastSource = null;
        List<T> lastResult = new();

        return new Computed<IEnumerable<T>>(() =>
        {
            // 1. Register Dependencies
            var mat = canvas.ViewMatrix.Value;
            var size = canvas.ViewportSize.Value;
            var items = source.Value;

            // 2. Threshold & Invalidation Check
            var dx = Math.Abs(mat.M31 - lastMat.M31);
            var dy = Math.Abs(mat.M32 - lastMat.M32);
            var ds = Math.Abs(mat.M11 - lastMat.M11);

            bool needsUpdate = items != lastSource || 
                               lastResult.Count == 0 ||
                               dx > moveThreshold || 
                               dy > moveThreshold || 
                               ds > scaleThreshold;

            if (!needsUpdate)
            {
                return lastResult;
            }

            // 3. Perform Filter
            lastMat = mat;
            lastSource = items;

            if (!mat.TryInvert(out var inv)) return items ?? Enumerable.Empty<T>();

            var viewRect = new Rect(0, 0, size.Width, size.Height);
            var worldRect = viewRect.TransformToAABB(inv).Inflate(buffer);

            return Sve.Untrack(() =>
            {
                if (items == null) return Enumerable.Empty<T>();
                
                var result = items.Where(item => isVisible(worldRect, item)).ToList();
                
                if (onVisibilityChanged != null)
                {
                    var oldSet = new HashSet<T>(lastResult);
                    var newSet = new HashSet<T>(result);

                    var entered = result.Where(x => !oldSet.Contains(x)).ToList();
                    var exited = lastResult.Where(x => !newSet.Contains(x)).ToList();

                    if (entered.Count > 0 || exited.Count > 0)
                    {
                        onVisibilityChanged(entered, exited);
                    }
                }

                lastResult = result;
                return result;
            });
        });
    }

    /// <summary>
    /// Convenience overload for AABB-based visibility.
    /// </summary>
    public static Computed<IEnumerable<T>> CreateVisibleSet<T>(
        InfiniteCanvas canvas,
        State<IEnumerable<T>> source,
        Func<T, Rect> boundsSelector,
        double moveThreshold = 50,
        double scaleThreshold = 0.1,
        double buffer = 1000,
        Action<IEnumerable<T>, IEnumerable<T>>? onVisibilityChanged = null)
    {
        return CreateVisibleSet(
            canvas, 
            source, 
            (worldRect, item) => worldRect.Intersects(boundsSelector(item)),
            moveThreshold, 
            scaleThreshold, 
            buffer,
            onVisibilityChanged);
    }
}