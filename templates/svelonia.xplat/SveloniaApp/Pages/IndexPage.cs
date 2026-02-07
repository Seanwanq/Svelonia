using Avalonia.Controls;
using Avalonia.Media;
using Svelonia.Fluent;
using Svelonia.Kit;

namespace SveloniaApp.Pages;

public class IndexPage : Page
{
    public IndexPage()
    {
        Title = "Welcome to Svelonia";

        Content = new StackPanel()
            .SetVerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
            .SetHorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
            .SetSpacing(20)
            .SetChildren(
                new TextBlock()
                    .SetText("Svelonia Cross-Platform")
                    .SetFontSize(24)
                    .SetFontWeight(FontWeight.Bold),
                new TextBlock()
                    .SetText("Runs on Desktop, Android, and Web!")
            );
    }
}
