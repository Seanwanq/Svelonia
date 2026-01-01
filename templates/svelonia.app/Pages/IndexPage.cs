using Avalonia.Controls;
using Avalonia.Layout;
using Svelonia.Kit;
using Svelonia.Core;
using Svelonia.Fluent;


namespace SveloniaApp.Pages;

using SveloniaApp;

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
                            .SetFontWeight(Avalonia.Media.FontWeight.Black)
                            .Fg(Sve.Res("TextColor"))
                            .SetText("Welcome to Svelonia")
                            .SetHorizontalAlignment(HorizontalAlignment.Center),

                        new Border()
                            .SetWidth(400)
                            .SetPadding(40)
                            .SetCornerRadius(15)
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
                                            .SetCornerRadius(10)
                                            .SetChild(
                                                new Button()
                                                    .SetContent("Explore Demo")
                                                    .SetPadding(20, 10)
                                                    .Bg(Sve.Res("PrimaryColor"))
                                                    .Fg(Avalonia.Media.Brushes.White)
                                                    .SetCornerRadius(10)
                                                    .OnClick(_ => App.Router.Navigate("/home"))
                                            )
                                    )
                            )
                    )
            );
    }
}
