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

    /// <summary>
    /// 
    /// </summary>
    public State<Control?> CurrentView { get; } = new(null);

    /// <summary>
    /// 
    /// </summary>
    public State<bool> IsLoading { get; } = new(false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="factory"></param>
    public void Register(string pattern, Func<RouteParams, Task<Component>> factory)
    {
        _routes.Add(new RouteEntry(pattern, factory));
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
        // 1. Run Guards
        foreach (var guard in _guards)
        {
            if (!await guard(fullPath)) return; // Guard cancelled navigation
        }

        IsLoading.Value = true;
        try
        {
            // 2. Parse Query Params
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

        public RouteEntry(string pattern, Func<RouteParams, Task<Component>> factory)
        {
            Factory = factory;
            var escaped = Regex.Escape(pattern).Replace(@"\{", "{").Replace(@"\}", "}");
            var regexPattern = "^" + Regex.Replace(escaped, @"\{(\w+)\}", "(?<$1>[^/]+)") + "$";
            Regex = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
