using Avalonia.Controls;
using Avalonia.Layout;
using Svelonia.Core;
using Svelonia.Fluent;

using Svelonia.Kit;

namespace SveloniaApp.Pages;

public class HomePage : Page
{
    private State<int> _count = new(0);

    public HomePage()
    {
        Content = new Border()
            .Bg(Sve.Res("BackgroundColor"))
            .SetChild(
                new StackPanel()
                    .SetHorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .SetVerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                    .SetSpacing(20)
                    .SetChildren(
                        new TextBlock()
                            .SetText("Counter")
                            .SetFontSize(24)
                            .SetFontWeight(Avalonia.Media.FontWeight.Bold)
                            .Fg(Sve.Res("TextColor"))
                            .SetHorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),

                        new Border()
                            .SetPadding(30)
                            .SetCornerRadius(10)
                            .Bg(Sve.Res("PaperBg"))
                            .SetBoxShadow(Sve.Res("ButtonShadowPressed"))
                            .SetChild(
                                new TextBlock()
                                    .BindText(new Computed<string>(() => $"Count: {_count.Value}"))
                                    .SetFontSize(32)
                                    .Fg(Sve.Res("PrimaryColor"))
                                    .SetHorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                            ),

                        new Border()
                            .SetBoxShadow(Sve.Res("ButtonShadow"))
                            .SetCornerRadius(10)
                            .SetChild(
                                new Button()
                                    .SetContent("Increment")
                                    .SetPadding(20, 10)
                                    .Bg(Sve.Res("PrimaryColor"))
                                    .Fg(Avalonia.Media.Brushes.White)
                                    .OnClick(_ => _count.Value++)
                                    .SetHorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                            ),

                        new Button()
                            .SetContent("Back to Welcome")
                            .Bg(Avalonia.Media.Brushes.Transparent)
                            .Fg(Sve.Res("TextColor"))
                            .OnClick(_ => App.Router.Navigate("/"))
                    )
            );
    }
}
