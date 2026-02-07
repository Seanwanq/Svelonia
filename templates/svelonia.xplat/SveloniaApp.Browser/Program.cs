using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using SveloniaApp;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static void Main(string[] args) => BuildAvaloniaApp()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
