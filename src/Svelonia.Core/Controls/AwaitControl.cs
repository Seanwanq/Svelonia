using Avalonia.Controls;
using Avalonia.Threading;

namespace Svelonia.Core.Controls;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class AwaitControl<T> : UserControl
{
    private readonly Func<Task<T>> _taskFactory;
    private readonly Func<Control>? _loading;
    private readonly Func<T, Control> _then;
    private readonly Func<Exception, Control>? _error;

    /// <summary>
    /// Creates an AwaitControl with a task factory, allowing for reloads.
    /// </summary>
    public AwaitControl(
        Func<Task<T>> taskFactory,
        Func<Control>? loading,
        Func<T, Control> then,
        Func<Exception, Control>? error)
    {
        _taskFactory = taskFactory;
        _loading = loading;
        _then = then;
        _error = error;

        Reload();
    }

    /// <summary>
    /// Compatibility constructor for a single-shot task.
    /// </summary>
    public AwaitControl(
        Task<T> task,
        Func<Control>? loading,
        Func<T, Control> then,
        Func<Exception, Control>? error) 
        : this(() => task, loading, then, error)
    {
    }

    /// <summary>
    /// Re-executes the task and refreshes the view.
    /// </summary>
    public void Reload()
    {
        // 1. Show Loading immediately
        if (_loading != null)
        {
            Content = _loading();
        }

        // 2. Run Task safely
        RunTask(_taskFactory());
    }

    private async void RunTask(Task<T> task)
    {
        try
        {
            var result = await task;

            // Switch to Success View
            Dispatcher.UIThread.Invoke(() =>
            {
                Content = _then(result);
            });
        }
        catch (Exception ex)
        {
            // Switch to Error View
            Dispatcher.UIThread.Invoke(() =>
            {
                if (_error != null)
                {
                    Content = _error(ex);
                }
                else
                {
                    // Default error view if none provided
                    Content = new TextBlock { Text = $"Error: {ex.Message}", Foreground = Avalonia.Media.Brushes.Red };
                }
            });
        }
    }
}
