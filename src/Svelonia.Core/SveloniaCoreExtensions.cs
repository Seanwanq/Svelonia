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
}
