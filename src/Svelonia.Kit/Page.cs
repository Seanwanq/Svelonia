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
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public virtual Task OnLoadAsync(RouteParams p) => Task.CompletedTask;
}