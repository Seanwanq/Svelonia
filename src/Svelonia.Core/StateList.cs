using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Threading;

namespace Svelonia.Core;

public class StateList<T> : ObservableCollection<T>, IDependency, IEnumerable<T>, System.Collections.IEnumerable
{
    private readonly HashSet<IObserver> _observers = new();

    /// <summary>
    /// A reactive state that increments whenever the collection changes.
    /// Accessing Version.Value inside a Computed will register a dependency on the collection structure.
    /// </summary>
    public State<int> Version { get; } = new(0);

    public StateList() { }
    public StateList(IEnumerable<T> collection) : base(collection) { }

    public StateList<T> Track()
    {
        ObserverContext.Current?.RegisterDependency(this);
        return this;
    }

    public new int Count
    {
        get
        {
            Track();
            var _ = Version.Value; // Also track version for structural changes
            return base.Count;
        }
    }

    public new T this[int index]
    {
        get
        {
            Track();
            var _ = Version.Value;
            return base[index];
        }
        set => base[index] = value;
    }

    public new IEnumerator<T> GetEnumerator()
    {
        Track();
        var _ = Version.Value;
        return base.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        Track();
        var _ = Version.Value;
        return base.GetEnumerator();
    }

    public IDisposable Subscribe(IObserver observer)
    {
        ((IDependency)this).Subscribe(observer);
        return new DisposableSubscription(this, observer);
    }

    private class DisposableSubscription(IDependency list, IObserver observer) : IDisposable
    {
        public void Dispose() => list.Unsubscribe(observer);
    }

    void IDependency.Subscribe(IObserver observer) => _observers.Add(observer);
    void IDependency.Unsubscribe(IObserver observer) => _observers.Remove(observer);

    private void NotifyObservers()
    {
        Version.Value++; // Increment version on every notification
        
        if (_observers.Count == 0) return;
        var targets = _observers.ToList();
        foreach (var observer in targets)
        {
            observer.OnStateChanged();
        }
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        if (items == null) return;

        RunOnUIThread(() => {
            var newList = items.ToList();
            Items.Clear();
            foreach (var item in newList)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            
            NotifyObservers();
        });
    }

    private void RunOnUIThread(Action action)
    {
        // 关键修复：如果在 UI 线程，必须同步执行，否则会产生时序 Bug
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Post(action);
        }
    }

    protected override void InsertItem(int index, T item)
    {
        RunOnUIThread(() => {
            base.InsertItem(index, item);
            NotifyObservers();
        });
    }

    protected override void RemoveItem(int index)
    {
        RunOnUIThread(() => {
            base.RemoveItem(index);
            NotifyObservers();
        });
    }

    protected override void ClearItems()
    {
        RunOnUIThread(() => {
            base.ClearItems();
            NotifyObservers();
        });
    }

    protected override void SetItem(int index, T item)
    {
        RunOnUIThread(() => {
            base.SetItem(index, item);
            NotifyObservers();
        });
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null) return;
        RunOnUIThread(() => {
            var itemList = items.ToList();
            if (itemList.Count == 0) return;
            foreach (var item in itemList) Items.Add(item);
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            NotifyObservers();
        });
    }
}
