using Avalonia.Controls;
using Avalonia.Layout;
using Svelonia.Core;
using Svelonia.Fluent;
using Svelonia.Generated;
using Svelonia.Kit;

namespace SveloniaApp.Pages;

public class HomePage : Page
{
    private State<int> _count = new(0);

    public HomePage()
    {
        Content = new Border()
            .Background(Sve.Res("BackgroundColor"))
            .Child(
                new StackPanel()
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                    .Spacing(20)
                    .Children(
                        new TextBlock()
                            .Text("Counter")
                            .FontSize(24)
                            .FontWeight(Avalonia.Media.FontWeight.Bold)
                            .Foreground(Sve.Res("TextColor"))
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),

                        new Border()
                            .Padding(30)
                            .CornerRadius(10)
                            .Background(Sve.Res("PaperBg"))
                            .BoxShadow(Sve.Res("ButtonShadowPressed"))
                            .Child(
                                new TextBlock()
                                    .BindTextContent(new Computed<string>(() => $"Count: {_count.Value}"))
                                    .FontSize(32)
                                    .Foreground(Sve.Res("PrimaryColor"))
                                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                            ),

                        new Border()
                            .BoxShadow(Sve.Res("ButtonShadow"))
                            .CornerRadius(10)
                            .Child(
                                new Button()
                                    .Content("Increment")
                                    .Padding(20, 10)
                                    .Background(Sve.Res("PrimaryColor"))
                                    .Foreground(Avalonia.Media.Brushes.White)
                                    .OnClick(_ => _count.Value++)
                                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                            ),

                        new Button()
                            .Content("Back to Welcome")
                            .Background(Avalonia.Media.Brushes.Transparent)
                            .Foreground(Sve.Res("TextColor"))
                            .OnClick(_ => App.Router.Navigate("/"))
                    )
            );
    }
}
