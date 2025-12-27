using System.ComponentModel;

namespace Svelonia.Kit;

/// <summary>
/// 
/// </summary>
public class RouteParams : Dictionary<string, string>
{
}

/// <summary>
/// 
/// </summary>
public class Page : Svelonia.Core.Component
{
    /// <summary>
    /// Gets or sets the title of the page.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Called before the page is navigated away from.
    /// Return false to prevent navigation.
    /// </summary>
    /// <returns></returns>
    public virtual Task<bool> CanLeaveAsync() => Task.FromResult(true);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public virtual Task OnLoadAsync(RouteParams p) => Task.CompletedTask;
}