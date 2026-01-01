using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Svelonia.Core;
using Svelonia.Data;
using Svelonia.Fluent;
using Svelonia.Generated;
using Svelonia.Kit;
using ISveApp = Svelonia.Core.ISveloniaApplication;

namespace SveloniaApp;

public class App : Application, ISveApp
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
            collection.AddSvelonia();

            collection.AddSingleton(Router);

            Services = collection.BuildServiceProvider();
            Sve.SetServiceProvider(Services);

            // Setup Routes (Source Generator)
            RouteRegistry.RegisterRoutes(Router, Services);

            // Setup Window
            Router.Navigate("/");

            desktop.MainWindow = new Window
            {
                Content = new NavigationHost(Router),
                Title = "Svelonia App",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 900,
                Height = 600
            };

            // Register Resize Listener for Responsive Design (MediaQuery)
            desktop.MainWindow.SizeChanged += (s, e) => MediaQuery.Update(e.NewSize);

#if DEBUG
            Svelonia.DevTools.SveloniaDevTools.Enable();
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }
}