using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Svelonia.Core;
using Svelonia.Core.Data;
using Svelonia.Kit;
using Svelonia.Generated;
using Microsoft.Extensions.DependencyInjection;
using System;
using Avalonia.Themes.Fluent;

namespace SveloniaApp;

public class App : Application
{
    public static Router Router { get; } = new();
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var collection = new ServiceCollection();

            // Register Framework Services
            collection.AddSveloniaDataAot();
            collection.AddSveloniaServices();
            collection.AddSingleton(Router);

            Services = collection.BuildServiceProvider();

            // Setup Routes
            RouteRegistry.RegisterRoutes(Router, Services);

            // Setup Window
            Router.Navigate("/");

            desktop.MainWindow = new Window
            {
                Content = new NavigationHost(Router),
                Title = "Svelonia App Template"
            };

#if DEBUG
            Svelonia.DevTools.SveloniaDevTools.Enable();
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }
}
