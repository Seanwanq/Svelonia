using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Svelonia.Core;

namespace Svelonia.Controls;

/// <summary>
/// A generic manager for handling single and multiple selection, including box (marquee) selection.
/// </summary>
/// <typeparam name="T">The type of selectable items.</typeparam>
public class SelectionManager<T> where T : class
{
    private readonly Func<IEnumerable<T>> _sourceSelector;
    private readonly Func<T, Rect> _boundsSelector;
    private readonly Action<T, bool> _setSelected;

    /// <summary>
    /// The collection of currently selected items.
    /// </summary>
    public StateList<T> SelectedItems { get; } = new();

    /// <summary>
    /// Creates a new SelectionManager.
    /// </summary>
    /// <param name="sourceSelector">Function to get the list of all candidates.</param>
    /// <param name="boundsSelector">Function to get the bounding box of an item in world coordinates.</param>
    /// <param name="setSelected">Action to update the 'IsSelected' state on the item itself.</param>
    public SelectionManager(
        Func<IEnumerable<T>> sourceSelector,
        Func<T, Rect> boundsSelector,
        Action<T, bool> setSelected)
    {
        _sourceSelector = sourceSelector;
        _boundsSelector = boundsSelector;
        _setSelected = setSelected;
    }

        /// <summary>
        /// Clears the entire selection.
        /// </summary>
        public void Clear()
        {
            Sve.Batch(() =>
            {
                foreach (var item in SelectedItems.ToList())
                {
                    _setSelected(item, false);
                }
                SelectedItems.Clear();
            });
        }
    
        /// <summary>
        /// Handles a single item click with modifier support.
        /// </summary>
        public void HandleClick(T? item, bool isCtrlPressed = false, bool isShiftPressed = false)
        {
            Sve.Batch(() =>
            {
                if (item == null)
                {
                    if (!isCtrlPressed) Clear();
                    return;
                }
    
                bool isAlreadySelected = SelectedItems.Contains(item);
    
                if (isCtrlPressed)
                {
                    // Toggle mode
                    if (isAlreadySelected)
                    {
                        SelectedItems.Remove(item);
                        _setSelected(item, false);
                    }
                    else
                    {
                        SelectedItems.Add(item);
                        _setSelected(item, true);
                    }
                }
                else
                {
                    // Normal single select
                    Clear();
                    SelectedItems.Add(item);
                    _setSelected(item, true);
                }
            });
        }
    
        /// <summary>
        /// Updates selection based on a bounding rectangle (Marquee Selection).
        /// </summary>
        /// <param name="selectionRect">The selection rectangle in world coordinates.</param>
        /// <param name="isIncremental">If true, items outside the rect are not deselected.</param>
        public void UpdateBoxSelection(Rect selectionRect, bool isIncremental = false)
        {
            Sve.Batch(() =>
            {
                var allItems = _sourceSelector();
    
                foreach (var item in allItems)
                {
                    var itemBounds = _boundsSelector(item);
                    bool isInside = selectionRect.Intersects(itemBounds);
                    bool isCurrent = SelectedItems.Contains(item);
    
                    if (isInside)
                    {
                        if (!isCurrent)
                        {
                            SelectedItems.Add(item);
                            _setSelected(item, true);
                        }
                    }
                    else if (!isIncremental)
                    {
                        if (isCurrent)
                        {
                            SelectedItems.Remove(item);
                            _setSelected(item, false);
                        }
                    }
                }
            });
        }
    
        /// <summary>
        /// Selects all items in the source.
        /// </summary>
        public void SelectAll()
        {
            Sve.Batch(() =>
            {
                foreach (var item in _sourceSelector())
                {
                    if (!SelectedItems.Contains(item))
                    {
                        SelectedItems.Add(item);
                        _setSelected(item, true);
                    }
                }
            });
        }}
