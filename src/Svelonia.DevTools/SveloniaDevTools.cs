using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace Svelonia.DevTools;

/// <summary>
/// 
/// </summary>
public static class SveloniaDevTools
{
    /// <summary>
    /// 
    /// </summary>
    public static void Enable()
    {
        DevToolsContext.Instance.Enable();

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow != null)
            {
                Attach(desktop.MainWindow);
            }
        }
    }

    private static void Attach(Window window)
    {
        window.KeyDown += (s, e) =>
        {
            if (e.Key == Key.F12)
            {
                new DevToolsWindow().Show();
            }
        };
    }
}
