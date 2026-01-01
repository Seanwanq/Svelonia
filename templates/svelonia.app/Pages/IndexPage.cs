using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Svelonia.Kit;
using Svelonia.Core;
using Svelonia.Fluent;

namespace SveloniaApp.Pages;

public class IndexPage : Page
{
    public IndexPage()
    {
        Content = new Border()
            .Bg(Sve.Res("BackgroundColor"))
            .SetChild(
                new StackPanel()
                    .SetVerticalAlignment(VerticalAlignment.Center)
                    .SetSpacing(30)
                    .SetChildren(
                        new TextBlock()
                            .SetFontSize(32)
                            .SetFontWeight(FontWeight.Black)
                            .Fg(Sve.Res("TextColor"))
                            .SetText("Welcome to Svelonia")
                            .SetHorizontalAlignment(HorizontalAlignment.Center),

                        new Border()
                            .SetWidth(400)
                            .P(40)
                            .Rounded(15)
                            .Bg(Sve.Res("PaperBg"))
                            .SetBoxShadow(Sve.Res("PaperShadow"))
                            .SetHorizontalAlignment(HorizontalAlignment.Center)
                            .SetChild(
                                new StackPanel()
                                    .SetSpacing(20)
                                    .SetChildren(
                                        new TextBlock()
                                            .SetText("Start building something beautiful.")
                                            .Fg(Sve.Res("TextColor"))
                                            .SetHorizontalAlignment(HorizontalAlignment.Center),

                                        new Border()
                                            .SetHorizontalAlignment(HorizontalAlignment.Center)
                                            .SetBoxShadow(Sve.Res("ButtonShadow"))
                                            .Rounded(10)
                                            .SetChild(
                                                new Button()
                                                    .SetContent("Explore Demo")
                                                    .P(20, 10)
                                                    .Bg(Sve.Res("PrimaryColor"))
                                                    .Fg(Brushes.White)
                                                    .Rounded(10)
                                                    .OnClick(_ => App.Router.Navigate("/home"))
                                            )
                                    )
                            ),
                            
                        new TextBlock()
                            .SetText("Press Ctrl+F12 to open DevTools")
                            .SetFontSize(12)
                            .Fg(Brushes.Gray)
                            .SetHorizontalAlignment(HorizontalAlignment.Center)
                    )
            );
    }
}