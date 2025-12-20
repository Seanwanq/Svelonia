using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Svelonia.Core.Data;

/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblyToScan"></param>
    /// <returns></returns>
    public static IServiceCollection AddSveloniaData(this IServiceCollection services, Assembly assemblyToScan)
    {
        services.AddSingleton<IMediator, Mediator>();

        // Scan for handlers
        var types = assemblyToScan.GetTypes();
        foreach (var type in types)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    {
                        services.AddTransient(i, type);
                    }
                }
            }
        }
        return services;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSveloniaServices(this IServiceCollection services)
    {
        services.AddSingleton<Services.IDialogService, Services.DialogService>();
        return services;
    }
}