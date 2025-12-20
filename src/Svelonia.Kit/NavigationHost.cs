using Avalonia.Controls;

namespace Svelonia.Kit;

/// <summary>
/// 
/// </summary>
public class NavigationHost : ContentControl
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="router"></param>
    public NavigationHost(Router router)
    {
        void Handler(Control? view) => Content = view;

        Content = router.CurrentView.Value;
        router.CurrentView.OnChange += Handler;

        DetachedFromVisualTree += (s, e) =>
        {
            router.CurrentView.OnChange -= Handler;
        };
    }
}
