using Avalonia;
using Avalonia.Controls;

namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public class Component : UserControl, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _isDisposed;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subscription"></param>
    protected void Track(IDisposable subscription)
    {
        _disposables.Add(subscription);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        foreach (var d in _disposables)
        {
            d.Dispose();
        }
        _disposables.Clear();

        OnDispose();
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnDispose() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        Dispose();
    }
}