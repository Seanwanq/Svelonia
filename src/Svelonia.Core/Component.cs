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
        
        CheckUntrackedDisposables();
        
        _isDisposed = true;

        foreach (var d in _disposables)
        {
            d.Dispose();
        }
        _disposables.Clear();

        OnDispose();
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void CheckUntrackedDisposables()
    {
        var type = GetType();
        var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        
        foreach (var field in fields)
        {
            if (typeof(IDisposable).IsAssignableFrom(field.FieldType))
            {
                try
                {
                    var value = field.GetValue(this) as IDisposable;
                    if (value != null && !_disposables.Contains(value))
                    {
                        // Skip Visuals (Controls) as they are usually managed by the Visual Tree
                        if (value is Avalonia.Visual) continue;
                        
                        // Skip Self
                        if (ReferenceEquals(value, this)) continue;

                        System.Diagnostics.Debug.WriteLine($"[Svelonia Warning] Component '{type.Name}' has an IDisposable field '{field.Name}' ({value.GetType().Name}) that is not being tracked. Call Track() to ensure cleanup.");
                    }
                }
                catch { /* Ignore reflection errors */ }
            }
        }
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