using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Svelonia.Core;

/// <summary>
/// 
/// </summary>
public static class SveloniaCoreExtensions
{
    /// <summary>
    /// Registers core services like DialogService.
    /// </summary>
    public static IServiceCollection AddSveloniaCore(this IServiceCollection services)
    {
        services.AddSingleton<Services.IDialogService, Services.DialogService>();
        return services;
    }

    /// <summary>
    /// Combined registration for Svelonia Core and standard dependencies
    /// </summary>
    public static IServiceCollection AddSvelonia(this IServiceCollection services)
    {
        services.AddSveloniaCore();
        return services;
    }

    /// <summary>
    /// Fluently load styles into the application
    /// </summary>
    public static Application LoadStyles(this Application app, params Action<Application>[] loaders)
    {
        foreach (var loader in loaders)
        {
            loader(app);
        }
        return app;
    }

    /// <summary>
    /// Registers a disposable resource to be disposed when the control is detached from the visual tree.
    /// </summary>
    public static T RegisterDisposable<T>(this T control, IDisposable disposable) where T : Control
    {
        control.DetachedFromVisualTree += (s, e) => disposable.Dispose();
        return control;
    }

    /// <summary>
    /// Safely detaches a control from its parent (Panel, Decorator, or ContentControl)
    /// </summary>
    public static void DetachFromParent(this Control? control)
    {
        if (control == null) return;
        if (control.Parent is Panel p)
        {
            p.Children.Remove(control);
        }
        else if (control.Parent is Decorator d)
        {
            d.Child = null;
        }
        else if (control.Parent is ContentControl cc)
        {
            cc.Content = null;
        }
    }
}
