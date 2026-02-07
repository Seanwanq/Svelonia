using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Svelonia.Core;
using Svelonia.Fluent;
using Svelonia.Kit;
using Svelonia.Generated;

namespace SveloniaApp;

public class App : Application
{
    public static Router Router { get; } = new();

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection()
            .AddSvelonia()
            .AddSingleton(Router)
            .BuildServiceProvider();

        RouteRegistry.RegisterRoutes(Router, services);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Router.Navigate("/");
            desktop.MainWindow = new Window
            {
                Title = "SveloniaApp",
                Content = new NavigationHost(Router)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new NavigationHost(Router);
            Router.Navigate("/");
        }

        base.OnFrameworkInitializationCompleted();
    }
}
