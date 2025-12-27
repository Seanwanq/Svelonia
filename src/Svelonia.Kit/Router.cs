using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Media;
using Svelonia.Core;

namespace Svelonia.Kit;

/// <summary>
/// Guard delegate: returns true to continue, false to cancel
/// </summary>
/// <param name="path"></param>
/// <returns></returns>
public delegate Task<bool> RouteGuard(string path);

/// <summary>
/// 
/// </summary>
public class Router
{
    private readonly List<RouteEntry> _routes = new();
    private readonly List<RouteGuard> _guards = new();
    private readonly Dictionary<string, Component> _cache = new();
    private string _currentPath = "/";

    /// <summary>
    /// 
    /// </summary>
    public Router()
    {
        HotReloadManager.OnRequestReload += () =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => Reload());
        };
    }

    /// <summary>
    /// Reloads the current route.
    /// </summary>
    public void Reload()
    {
        if (!string.IsNullOrEmpty(_currentPath))
        {
            // Clear cache for current path on reload to ensure fresh component
            _cache.Remove(_currentPath);
            Navigate(_currentPath);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public State<Control?> CurrentView { get; } = new(null);

    /// <summary>
    /// 
    /// </summary>
    public State<bool> IsLoading { get; } = new(false);

    /// <summary>
    /// Registers a route.
    /// </summary>
    /// <param name="pattern">Route pattern (e.g. /user/{id})</param>
    /// <param name="factory">Component factory</param>
    /// <param name="keepAlive">Whether to cache the component instance</param>
    public void Register(string pattern, Func<RouteParams, Task<Component>> factory, bool keepAlive = false)
    {
        _routes.Add(new RouteEntry(pattern, factory, keepAlive));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guard"></param>
    public void AddGuard(RouteGuard guard)
    {
        _guards.Add(guard);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fullPath"></param>
    public async void Navigate(string fullPath)
    {
        Console.WriteLine($"[Router] Request Navigate: {fullPath}");

        // 0. Run Page-level Guards
        var activePage = GetActivePage(CurrentView.Value as Component);
        if (activePage != null)
        {
            if (!await activePage.CanLeaveAsync()) 
            {
                Console.WriteLine("[Router] Page Guard prevented navigation.");
                return;
            }
        }

        string oldBasePath = _currentPath.Split('?')[0];
        _currentPath = fullPath;

        // 1. Run Global Guards
        foreach (var guard in _guards)
        {
            if (!await guard(fullPath)) 
            {
                Console.WriteLine("[Router] Global Guard prevented navigation.");
                return; 
            }
        }

        Console.WriteLine("[Router] Guards passed.");

        // Parse Params early for OnLoadAsync
        var p = new RouteParams();
        var parts = fullPath.Split('?');
        var path = parts[0];
        var query = parts.Length > 1 ? parts[1] : "";
        if (!string.IsNullOrEmpty(query))
        {
            var qParts = query.Split('&');
            foreach (var qp in qParts)
            {
                var kv = qp.Split('=');
                if (kv.Length == 2) p[kv[0]] = Uri.UnescapeDataString(kv[1]);
            }
        }

        // 2. Check Cache
        if (_cache.TryGetValue(fullPath, out var cachedComponent))
        {
            Console.WriteLine("[Router] Cache hit.");
            var cachedPage = GetActivePage(cachedComponent);
            if (cachedPage != null)
            {
                await cachedPage.OnLoadAsync(p);
            }
            CurrentView.Value = cachedComponent;
            return;
        }

        IsLoading.Value = true;
        try
        {
            foreach (var route in _routes)
            {
                var match = route.Regex.Match(path);
                if (match.Success)
                {
                    // Extract Route Params
                    foreach (Group group in match.Groups)
                    {
                        if (!int.TryParse(group.Name, out _)) p[group.Name] = group.Value;
                    }

                    // Create or Reuse Component
                    Component? component = null;
                    bool isSameRoute = (path == oldBasePath); 

                    if (isSameRoute && CurrentView.Value is Component currentComp && route.KeepAlive)
                    {
                        Console.WriteLine($"[Router] In-place update for {currentComp.GetType().Name}");
                        component = currentComp;
                    }
                    else
                    {
                        Console.WriteLine($"[Router] Creating new component via factory.");
                        component = await route.Factory(p);
                        if (route.KeepAlive) _cache[fullPath] = component;
                    }

                    // 4. Update Parameters and Load
                    var targetPage = GetActivePage(component);
                    if (targetPage != null)
                    {
                        Console.WriteLine($"[Router] Calling OnLoadAsync on {targetPage.GetType().Name}.");
                        try 
                        {
                            await targetPage.OnLoadAsync(p);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Router] Error in OnLoadAsync for {targetPage.GetType().Name}: {ex.Message}");
                        }
                    }

                    if (CurrentView.Value != component)
                    {
                        Console.WriteLine("[Router] Switching View.");
                        CurrentView.Value = component;
                    }
                    return;
                }
            }

            CurrentView.Value = new TextBlock { Text = $"404 Not Found: {fullPath}" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Router] Critical Navigation Error: {ex.Message}");
            CurrentView.Value = new TextBlock { Text = $"Navigation Error: {ex.Message}", Foreground = Brushes.Red };
        }
        finally
        {
            IsLoading.Value = false;
        }
    }

    private Page? GetActivePage(Component? component)
    {
        if (component is Page p) return p;
        if (component is Layout l && l.Slot is Component c) return GetActivePage(c);
        return null;
    }

    private class RouteEntry
    {
        public Regex Regex { get; }
        public Func<RouteParams, Task<Component>> Factory { get; }
        public bool KeepAlive { get; }

        public RouteEntry(string pattern, Func<RouteParams, Task<Component>> factory, bool keepAlive = false)
        {
            Factory = factory;
            KeepAlive = keepAlive;
            var escaped = Regex.Escape(pattern).Replace(@"\{", "{").Replace(@"\}", "}");
            var regexPattern = "^" + Regex.Replace(escaped, @"\{(\w+)\}", "(?<$1>[^/]+)") + "$";
            Regex = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
