using Avalonia.Controls;
using Avalonia.Layout;
using Svelonia.Core;
using Svelonia.Core.Bindings;
using Svelonia.Generated;
using Svelonia.Kit;

namespace Svelonia.App.Template.Pages;

public class HomePage : Page
{
    private State<int> _count = new(0);

    public HomePage()
    {
        Content = new StackPanel()
            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
            .Spacing(20)
            .Children(
                new TextBlock()
                    .BindTextContent(new Computed<string>(() => $"Current Count: {_count.Value}"))
                    .FontSize(18)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),

                new Button()
                    .Content("Increment")
                    .OnClick(_ => _count.Value++)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
            );
    }
}
