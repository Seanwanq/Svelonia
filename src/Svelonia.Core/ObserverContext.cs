namespace Svelonia.Core;

public interface IDependency
{
    void Subscribe(IObserver observer);
    void Unsubscribe(IObserver observer);
}

public interface IObserver
{
    void OnStateChanged();
    void RegisterDependency(IDependency dependency);
}

internal static class ObserverContext
{
    [ThreadStatic]
    private static Stack<IObserver>? _stack;

    [ThreadStatic]
    private static int _untrackCount;

    [ThreadStatic]
    private static int _batchCount;

    [ThreadStatic]
    private static HashSet<IObserver>? _dirtyObservers;

    public static bool IsBatching => _batchCount > 0;

    public static void Push(IObserver observer)
    {
        _stack ??= new Stack<IObserver>();
        _stack.Push(observer);
    }

    public static void Pop() => _stack?.Pop();

    public static void PushUntrack() => _untrackCount++;
    public static void PopUntrack() => _untrackCount--;

    public static void PushBatch() => _batchCount++;

    public static void PopBatch()
    {
        _batchCount--;
        if (_batchCount == 0 && _dirtyObservers != null)
        {
            var observers = _dirtyObservers.ToList();
            _dirtyObservers.Clear();
            foreach (var obs in observers)
            {
                obs.OnStateChanged();
            }
        }
    }

    public static void RegisterDirty(IObserver observer)
    {
        _dirtyObservers ??= new HashSet<IObserver>();
        _dirtyObservers.Add(observer);
    }

    // 关键修复：增加 BypassUntrack 模式，或者修改 Current 的逻辑
    // 当我们在 Effect 或 Computed 内部显式 Push 时，我们通常是希望追踪的。
    public static IObserver? Current => (_stack != null && _stack.Count > 0 && _untrackCount == 0) ? _stack.Peek() : null;

    // 为框架提供强制追踪的能力
    public static void ForceTrack(Action action)
    {
        int oldUntrack = _untrackCount;
        _untrackCount = 0;
        try { action(); }
        finally { _untrackCount = oldUntrack; }
    }
}
