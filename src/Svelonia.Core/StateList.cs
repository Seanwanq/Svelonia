using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Threading;

namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class StateList<T> : ObservableCollection<T>
{
    /// <summary>
    /// 
    /// </summary>
    public StateList() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collection"></param>
    public StateList(IEnumerable<T> collection) : base(collection) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    protected override void InsertItem(int index, T item)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => InsertItem(index, item));
            return;
        }
        base.InsertItem(index, item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    protected override void RemoveItem(int index)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => RemoveItem(index));
            return;
        }
        base.RemoveItem(index);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void ClearItems()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => ClearItems());
            return;
        }
        base.ClearItems();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    protected override void SetItem(int index, T item)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => SetItem(index, item));
            return;
        }
        base.SetItem(index, item);
    }

    /// <summary>
    /// Adds a range of items to the collection, triggering a single notification.
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        if (items == null) return;

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => AddRange(items));
            return;
        }

        var itemList = items.ToList();
        if (itemList.Count == 0) return;

        CheckReentrancy();

        var startIndex = Count;
        foreach (var item in itemList)
        {
            Items.Add(item);
        }

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemList, startIndex));
    }
}