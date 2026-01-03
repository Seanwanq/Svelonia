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

    public static void Push(IObserver observer)
    {
        _stack ??= new Stack<IObserver>();
        _stack.Push(observer);
    }

    public static void Pop() => _stack?.Pop();

    public static void PushUntrack() => _untrackCount++;
    public static void PopUntrack() => _untrackCount--;

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
