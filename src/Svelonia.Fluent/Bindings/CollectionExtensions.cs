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
    public static T MapToChildren<T, TData>(this T panel, StateList<TData> source, Func<TData, Control> factory) 
        where T : Panel
        where TData : notnull
    {
        var controlMap = new Dictionary<TData, Control>();
        bool isSubscribed = false;

        void Sync()
        {
            try
            {
                // Ensure we run on UI thread because we touch Visual Tree
                if (!Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(Sync);
                    return;
                }

                var currentDataItems = source.ToList();

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
                        if (oldIdx != -1) panel.Children.RemoveAt(oldIdx);
                        panel.Children.Insert(i, control);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MapToChildren] CRITICAL ERROR during Sync: {ex.Message}");
            }
        }

        void Handler(object? sender, NotifyCollectionChangedEventArgs e) => Sync();

        panel.AttachedToVisualTree += (s, e) => { 
            if (!isSubscribed) {
                source.CollectionChanged += Handler; 
                isSubscribed = true;
                Sync(); 
            }
        };
        
        panel.DetachedFromVisualTree += (s, e) => { 
            if (isSubscribed) {
                source.CollectionChanged -= Handler; 
                isSubscribed = false;
            }
        };

        // Manual initial sync if already loaded
        if (panel.IsLoaded && !isSubscribed) { 
            source.CollectionChanged += Handler; 
            isSubscribed = true;
            Sync(); 
        }

        return panel;
    }
}
