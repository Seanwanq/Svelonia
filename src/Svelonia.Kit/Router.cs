using System.Text.RegularExpressions;
using Avalonia.Controls;
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
        _currentPath = fullPath;

        // 1. Run Guards
        foreach (var guard in _guards)
        {
            if (!await guard(fullPath)) return; // Guard cancelled navigation
        }

        // 2. Check Cache
        if (_cache.TryGetValue(fullPath, out var cachedComponent))
        {
            CurrentView.Value = cachedComponent;
            return;
        }

        IsLoading.Value = true;
        try
        {
            // 3. Parse Query Params
            var parts = fullPath.Split('?');
            var path = parts[0];
            var query = parts.Length > 1 ? parts[1] : "";

            foreach (var route in _routes)
            {
                var match = route.Regex.Match(path);
                if (match.Success)
                {
                    var p = new RouteParams();

                    // Route Params (/user/{id})
                    foreach (Group group in match.Groups)
                    {
                        if (!int.TryParse(group.Name, out _))
                        {
                            p[group.Name] = group.Value;
                        }
                    }

                    // Query Params (?sort=asc)
                    if (!string.IsNullOrEmpty(query))
                    {
                        var qParts = query.Split('&');
                        foreach (var qp in qParts)
                        {
                            var kv = qp.Split('=');
                            if (kv.Length == 2)
                            {
                                p[kv[0]] = Uri.UnescapeDataString(kv[1]);
                            }
                        }
                    }

                    // Create Component
                    var component = await route.Factory(p);

                    // 4. Cache if keepAlive
                    if (route.KeepAlive)
                    {
                        _cache[fullPath] = component;
                    }

                    CurrentView.Value = component;
                    return;
                }
            }

            CurrentView.Value = new TextBlock { Text = $"404 Not Found: {fullPath}" };
        }
        finally
        {
            IsLoading.Value = false;
        }
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
