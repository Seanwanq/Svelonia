using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Svelonia.Core;

namespace Svelonia.Fluent;

public static class CollectionExtensions
{
    /// <summary>
    /// Synchronizes a Panel's children with a StateList using an extremely robust incremental algorithm.
    /// Preserves existing control instances and their states (like focus and cursor).
    /// </summary>
    public static T MapToChildren<T, TData>(
        this T panel,
        StateList<TData> source,
        Func<TData, Control> factory
    )
        where T : Panel
        where TData : notnull
    {
        var controlMap = new Dictionary<TData, Control>();
        bool isSubscribed = false;

        void Handler(object? sender, NotifyCollectionChangedEventArgs e) =>
            SyncList(panel, source.ToList(), controlMap, factory);

        panel.AttachedToVisualTree += (s, e) =>
        {
            if (!isSubscribed)
            {
                source.CollectionChanged += Handler;
                isSubscribed = true;
                SyncList(panel, source.ToList(), controlMap, factory);
            }
        };

        panel.DetachedFromVisualTree += (s, e) =>
        {
            if (isSubscribed)
            {
                source.CollectionChanged -= Handler;
                isSubscribed = false;
            }
        };

        // Manual initial sync if already loaded
        if (panel.IsLoaded && !isSubscribed)
        {
            source.CollectionChanged += Handler;
            isSubscribed = true;
            SyncList(panel, source.ToList(), controlMap, factory);
        }

        return panel;
    }

    /// <summary>
    /// Synchronizes a Panel's children with a Computed IEnumerable (e.g., filtered list).
    /// Automatically performs diffing when the computed value updates.
    /// </summary>
    public static T MapToChildren<T, TData>(
        this T panel,
        State<IEnumerable<TData>> source,
        Func<TData, Control> factory
    )
        where T : Panel
        where TData : notnull
    {
        var controlMap = new Dictionary<TData, Control>();

        // This effect will run whenever the source computed value changes.
        // It automatically handles the subscription/unsubscription lifecycle via Svelonia's Effect system,
        // but we need to ensure it's tied to the control's lifecycle to avoid memory leaks if the control is removed.
        // For now, we rely on the user to manage the scope or simple Effect usage.
        // Ideally, we should tie this to AttachedToVisualTree like above, but Effects are harder to pause.
        // Let's use a manual subscription to OnChange.

        bool isSubscribed = false;

        void Handler(IEnumerable<TData>? newList)
        {
            if (newList == null)
                return;
            SyncList(panel, newList.ToList(), controlMap, factory);
        }

        void OnChangeObject(object? val) => Handler(val as IEnumerable<TData>);

        panel.AttachedToVisualTree += (s, e) =>
        {
            if (!isSubscribed)
            {
                source.OnChangeObject += OnChangeObject;
                isSubscribed = true;
                Handler(source.Value);
            }
        };

        panel.DetachedFromVisualTree += (s, e) =>
        {
            if (isSubscribed)
            {
                source.OnChangeObject -= OnChangeObject;
                isSubscribed = false;
            }
        };

        if (panel.IsLoaded && !isSubscribed)
        {
            source.OnChangeObject += OnChangeObject;
            isSubscribed = true;
            Handler(source.Value);
        }

        return panel;
    }

    private static void SyncList<TData>(
        Panel panel,
        List<TData> currentDataItems,
        Dictionary<TData, Control> controlMap,
        Func<TData, Control> factory
    )
        where TData : notnull
    {
        try
        {
            // Ensure we run on UI thread because we touch Visual Tree
            if (!Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    SyncList(panel, currentDataItems, controlMap, factory)
                );
                return;
            }

            // 1. Identify and remove controls for data that no longer exists
            var currentDataHashSet = new HashSet<TData>(currentDataItems);
            var keysToRemove = controlMap.Keys.Where(k => !currentDataHashSet.Contains(k)).ToList();
            foreach (var k in keysToRemove)
            {
                var control = controlMap[k];
                panel.Children.Remove(control);
                controlMap.Remove(k);
            }

            // 2. Synchronize existing controls and add new ones
            for (int i = 0; i < currentDataItems.Count; i++)
            {
                var data = currentDataItems[i];
                if (!controlMap.TryGetValue(data, out var control))
                {
                    control = factory(data);
                    controlMap[data] = control;
                }

                // Ensure correct position in visual tree
                if (i >= panel.Children.Count)
                {
                    panel.Children.Add(control);
                }
                else if (panel.Children[i] != control)
                {
                    int oldIdx = panel.Children.IndexOf(control);
                    if (oldIdx != -1)
                        panel.Children.RemoveAt(oldIdx);
                    panel.Children.Insert(i, control);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapToChildren] CRITICAL ERROR during Sync: {ex.Message}");
        }
    }
}
