using Avalonia;
using Svelonia.Core;

namespace Svelonia.Controls;

public static class ReactiveViewport
{
    /// <summary>
    /// Creates a Computed IEnumerable that only contains items visible within the InfiniteCanvas viewport.
    /// </summary>
    /// <param name="viewport">The InfiniteCanvas to track.</param>
    /// <param name="source">The reactive source of items.</param>
    /// <param name="itemBoundsSelector">Function to get the bounding box of an item.</param>
    /// <param name="buffer">Padding around the viewport in logical pixels. Default 1000.</param>
    public static Computed<IEnumerable<T>> CreateVisibleSet<T>(
        InfiniteCanvas viewport,
        State<IEnumerable<T>> source,
        Func<T, Rect> itemBoundsSelector,
        double buffer = 1000
    )
    {
        return new Computed<IEnumerable<T>>(() =>
        {
            // 1. Track Dependencies
            var mat = viewport.ViewMatrix.Value;
            var size = viewport.ViewportSize.Value;
            var items = source.Value; // Track source replacement

            // 2. Calculate World Rect
            if (!mat.TryInvert(out var inv))
                return items ?? Enumerable.Empty<T>();

            var viewRect = new Rect(0, 0, size.Width, size.Height);
            var worldRect = viewRect.TransformToAABB(inv);
            worldRect = worldRect.Inflate(buffer);

            // 3. Filter (Untracked)
            return Sve.Untrack(() =>
            {
                if (items == null)
                    return Enumerable.Empty<T>();
                return items.Where(item => worldRect.Intersects(itemBoundsSelector(item)));
            });
        });
    }

    /// <summary>
    /// Creates a Computed IEnumerable from a StateList/IEnumerable with manual version tracking.
    /// Use this for StateList or non-reactive collections where you provide a version tick.
    /// </summary>
    public static Computed<IEnumerable<T>> CreateVisibleSet<T>(
        InfiniteCanvas viewport,
        IEnumerable<T> source,
        State<int> versionTrigger,
        Func<T, Rect> itemBoundsSelector,
        double buffer = 1000
    )
    {
        return new Computed<IEnumerable<T>>(() =>
        {
            // 1. Track Dependencies
            var mat = viewport.ViewMatrix.Value;
            var size = viewport.ViewportSize.Value;
            var _ = versionTrigger.Value; // Track version

            // 2. Calculate World Rect
            if (!mat.TryInvert(out var inv))
                return source;

            var viewRect = new Rect(0, 0, size.Width, size.Height);
            var worldRect = viewRect.TransformToAABB(inv);
            worldRect = worldRect.Inflate(buffer);

            // 3. Filter (Untracked)
            return Sve.Untrack(() =>
            {
                return source.Where(item => worldRect.Intersects(itemBoundsSelector(item)));
            });
        });
    }
}
