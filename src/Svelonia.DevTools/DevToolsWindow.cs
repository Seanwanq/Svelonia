using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Svelonia.Fluent;
using Svelonia.Generated;

namespace Svelonia.DevTools;

/// <summary>
/// 
/// </summary>
public class DevToolsWindow : Window
{
    /// <summary>
    /// 
    /// </summary>
    public DevToolsWindow()
    {
        Title = "Svelonia DevTools";
        Width = 600;
        Height = 400;

        Content = new Grid()
            .Rows("Auto,*")
            .Children(
                // Header
                new Border().Background(Brushes.LightGray).Padding(10).Child(
                    new TextBlock().Text("State Log").FontWeight(FontWeight.Bold)
                ).Row(0),

                // Log List
                new ScrollViewer().Content(
                    Core.Sve.Each(DevToolsContext.Instance.Logs, log =>
                        new Border()
                            .BorderThickness(0, 0, 0, 1)
                            .BorderBrush(Brushes.LightGray)
                            .Padding(5)
                            .Child(
                                new StackPanel().Orientation(Orientation.Horizontal).Spacing(10).Children(
                                    new TextBlock().Text(log.Timestamp.ToString("HH:mm:ss.fff")).Foreground(Brushes.Gray).FontSize(12),
                                    new TextBlock().Text(log.Source).FontWeight(FontWeight.SemiBold).Width(150),
                                    new TextBlock().Text(log.Value).TextWrapping(TextWrapping.Wrap)
                                )
                            )
                    )
                ).Row(1)
            );
    }
}
