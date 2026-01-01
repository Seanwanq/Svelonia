using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Svelonia.Core;
using Svelonia.Fluent;
using Svelonia.Kit;

namespace SveloniaApp.Pages;

public class HomePage : Page
{
    private readonly State<int> _count = new(0);

    public HomePage()
    {
        Title = "Home - Svelonia";

        Content = new Border()
            .Bg(Sve.Res("BackgroundColor"))
            .SetChild(
                new StackPanel()
                    .SetHorizontalAlignment(HorizontalAlignment.Center)
                    .SetVerticalAlignment(VerticalAlignment.Center)
                    .SetSpacing(20)
                    .SetChildren(
                        new TextBlock()
                            .SetText("Reactive Counter")
                            .SetFontSize(24)
                            .SetFontWeight(FontWeight.Bold)
                            .Fg(Sve.Res("TextColor"))
                            .SetHorizontalAlignment(HorizontalAlignment.Center),

                        new Border()
                            .P(30)
                            .Rounded(10)
                            .Bg(Sve.Res("PaperBg"))
                            .SetBoxShadow(Sve.Res("ButtonShadowPressed"))
                            .SetChild(
                                new TextBlock()
                                    .BindText(_count.Select(c => $"Count: {c}"))
                                    .SetFontSize(32)
                                    .Fg(Sve.Res("PrimaryColor"))
                                    .SetHorizontalAlignment(HorizontalAlignment.Center)      
                            ),

                        new StackPanel()
                            .SetOrientation(Orientation.Horizontal)
                            .SetSpacing(10)
                            .SetChildren(
                                new Border()
                                    .SetBoxShadow(Sve.Res("ButtonShadow"))
                                    .Rounded(10)
                                    .SetChild(
                                        new Button()
                                            .SetContent("Increment")
                                            .P(20, 10)
                                            .Bg(Sve.Res("PrimaryColor"))
                                            .Fg(Brushes.White)
                                            .Rounded(10)
                                            .OnClick(_ => _count.Value++)
                                    ),
                                    
                                new Button()
                                    .SetContent("Clear")
                                    .P(20, 10)
                                    .Bg(Brushes.Transparent)
                                    .Fg(Sve.Res("TextColor"))
                                    .OnClick(_ => _count.Value = 0)
                            ),

                        new Button()
                            .SetContent("Back to Welcome")
                            .Bg(Brushes.Transparent)
                            .Fg(Brushes.Gray)
                            .SetMargin(new Avalonia.Thickness(0, 20, 0, 0))
                            .OnClick(_ => App.Router.Navigate("/"))
                    )
            );
    }
}