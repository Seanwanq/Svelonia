using Avalonia.Controls;
using Avalonia.Layout;
using Svelonia.Kit;
using Svelonia.Core;
using Svelonia.Generated;

namespace SveloniaApp.Pages;

using SveloniaApp;

public class IndexPage : Page
{
    public IndexPage()
    {
        Content = new StackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .Spacing(20)
            .Children(
                new TextBlock()
                    .FontSize(24)
                    .Text("Welcome to Svelonia!")
                    .HorizontalAlignment(HorizontalAlignment.Center),
                new Button()
                    .Content("Go to Home Page")
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .OnClick(_ => App.Router.Navigate("/home"))
            );
    }
}