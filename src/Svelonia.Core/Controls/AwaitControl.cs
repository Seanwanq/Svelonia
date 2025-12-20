using Avalonia.Controls;
using Avalonia.Threading;

namespace Svelonia.Core.Controls;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class AwaitControl<T> : UserControl
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="loading"></param>
    /// <param name="then"></param>
    /// <param name="error"></param>
    public AwaitControl(
        Task<T> task,
        Func<Control>? loading,
        Func<T, Control> then,
        Func<Exception, Control>? error)
    {
        // 1. Show Loading immediately
        if (loading != null)
        {
            Content = loading();
        }

        // 2. Run Task safely
        // We don't await in constructor, we fire and forget but handle context
        RunTask(task, then, error);
    }

    private async void RunTask(Task<T> task, Func<T, Control> then, Func<Exception, Control>? error)
    {
        try
        {
            var result = await task;

            // Switch to Success View
            Dispatcher.UIThread.Invoke(() =>
            {
                Content = then(result);
            });
        }
        catch (Exception ex)
        {
            // Switch to Error View
            Dispatcher.UIThread.Invoke(() =>
            {
                if (error != null)
                {
                    Content = error(ex);
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
