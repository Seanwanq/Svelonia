using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Svelonia.Core;
using Svelonia.Fluent;

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
            .SetRows("Auto,*")
            .SetChildren(
                // Header
                new Border()
                    .Bg(Brushes.LightGray)
                    .SetPadding(10)
                    .SetChild(new TextBlock().SetText("State Log").SetFontWeight(FontWeight.Bold))
                    .SetRow(0),
                // Log List
                new ScrollViewer()
                    .SetContent(
                        Svelonia.Core.Sve.Each(
                            DevToolsContext.Instance.Logs,
                            log =>
                                new Border()
                                    .SetBorderThickness(0, 0, 0, 1)
                                    .SetBorderBrush(Brushes.LightGray)
                                    .SetPadding(5)
                                    .SetChild(
                                        new StackPanel()
                                            .SetOrientation(Orientation.Horizontal)
                                            .SetSpacing(10)
                                            .SetChildren(
                                                new TextBlock()
                                                    .SetText(log.Timestamp.ToString("HH:mm:ss.fff"))
                                                    .SetForeground(Brushes.Gray)
                                                    .SetFontSize(12),
                                                new TextBlock()
                                                    .SetText(log.Source)
                                                    .SetFontWeight(FontWeight.SemiBold)
                                                    .SetWidth(150),
                                                new TextBlock()
                                                    .SetText(log.Value)
                                                    .SetTextWrapping(TextWrapping.Wrap)
                                            )
                                    )
                        )
                    )
                    .SetRow(1)
            );
    }
}
