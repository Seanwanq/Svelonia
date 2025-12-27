using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Svelonia.Core;
using Svelonia.Data;
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

    public static readonly State<string> LightTheme = new("Themes/light.json");
    public static readonly State<string> DarkTheme = new("Themes/dark.json");

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        
        // Setup Svelonia Theme
        SveloniaTheme.Setup(this, LightTheme, DarkTheme);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var collection = new ServiceCollection();

            // Register Framework Services
            collection.AddSvelonia(); // Use combined AddSvelonia
            collection.AddSingleton(Router);

            Services = collection.BuildServiceProvider();
            Sve.SetServiceProvider(Services);

            // Setup Routes
            RouteRegistry.RegisterRoutes(Router, Services);

            // Setup Window
            Router.Navigate("/");

            desktop.MainWindow = new Window
            {
                Content = new NavigationHost(Router),
                Title = "Svelonia App Template",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 900,
                Height = 600
            };

#if DEBUG
            Svelonia.DevTools.SveloniaDevTools.Enable();
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }
}
