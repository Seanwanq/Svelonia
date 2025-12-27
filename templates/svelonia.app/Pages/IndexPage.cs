using Avalonia.Controls;
using Avalonia.Layout;
using Svelonia.Kit;
using Svelonia.Core;
using Svelonia.Fluent;
using Svelonia.Generated;

namespace SveloniaApp.Pages;

using SveloniaApp;

public class IndexPage : Page
{
    public IndexPage()
    {
        Content = new Border()
            .Background(Sve.Res("BackgroundColor"))
            .Child(
                new StackPanel()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Spacing(30)
                    .Children(
                        new TextBlock()
                            .FontSize(32)
                            .FontWeight(Avalonia.Media.FontWeight.Black)
                            .Foreground(Sve.Res("TextColor"))
                            .Text("Welcome to Svelonia")
                            .HorizontalAlignment(HorizontalAlignment.Center),

                        new Border()
                            .Width(400)
                            .Padding(40)
                            .CornerRadius(15)
                            .Background(Sve.Res("PaperBg"))
                            .BoxShadow(Sve.Res("PaperShadow"))
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Child(
                                new StackPanel()
                                    .Spacing(20)
                                    .Children(
                                        new TextBlock()
                                            .Text("Start building something beautiful.")
                                            .Foreground(Sve.Res("TextColor"))
                                            .HorizontalAlignment(HorizontalAlignment.Center),

                                        new Border()
                                            .HorizontalAlignment(HorizontalAlignment.Center)
                                            .BoxShadow(Sve.Res("ButtonShadow"))
                                            .CornerRadius(10)
                                            .Child(
                                                new Button()
                                                    .Content("Explore Demo")
                                                    .Padding(20, 10)
                                                    .Background(Sve.Res("PrimaryColor"))
                                                    .Foreground(Avalonia.Media.Brushes.White)
                                                    .CornerRadius(10)
                                                    .OnClick(_ => App.Router.Navigate("/home"))
                                            )
                                    )
                            )
                    )
            );
    }
}