using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;

namespace Svelonia.Core.Services;

/// <summary>
/// 
/// </summary>
public class DialogService : IDialogService
{
    private Window? GetOwner()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Simple strategy: use MainWindow.
            // In a real app, you might want the currently active window.
            return desktop.MainWindow;
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ShowAlertAsync(string title, string message)
    {
        var owner = GetOwner();
        if (owner == null) return;  // Cannot show dialog without owner in this simple impl

        var window = new Window
        {
            Title = title,
            Width = 300,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SystemDecorations = SystemDecorations.BorderOnly
        };

        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right
        };

        okButton.Click += (_, _) => window.Close();

        var content = new Border
        {
            Padding = new Thickness(20),
            Child = new StackPanel
            {
                Spacing = 20,
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontWeight = FontWeight.Bold,
                        FontSize = 16
                    },
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap
                    },
                    okButton
                }
            }
        };

        window.Content = content;
        await window.ShowDialog(owner);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="yesText"></param>
    /// <param name="noText"></param>
    /// <returns></returns>
    public async Task<bool> ShowConfirmAsync(string title, string message, string yesText = "Yes", string noText = "No")
    {
        var owner = GetOwner();
        if (owner == null) return false;

        var result = false;
        var window = new Window
        {
            Title = title,
            Width = 350,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SystemDecorations = SystemDecorations.BorderOnly
        };

        var noButton = new Button { Content = noText };
        noButton.Click += (_, _) => { result = false; window.Close(); };

        var yesButton = new Button { Content = yesText };
        yesButton.Click += (_, _) => { result = true; window.Close(); };

        var content = new Border
        {
            Padding = new Thickness(20),
            Child = new StackPanel
            {
                Spacing = 20,
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontWeight = FontWeight.Bold,
                        FontSize = 16
                    },
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Children =
                        {
                            noButton,
                            yesButton
                        }
                    }
                }
            }
        };

        window.Content = content;
        await window.ShowDialog(owner);
        return result;
    }
}