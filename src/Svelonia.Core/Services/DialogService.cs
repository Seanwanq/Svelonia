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
    /// <param name="okText"></param>
    /// <param name="cancelText"></param>
    /// <param name="altText"></param>
    /// <returns></returns>
    public async Task<DialogResult> ShowConfirmAsync(string title, string message, string okText = "Yes", string cancelText = "No", string? altText = null)
    {
        var owner = GetOwner();
        if (owner == null) return DialogResult.None;

        var result = DialogResult.None;
        var window = new Window
        {
            Title = title,
            Width = 350,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            SystemDecorations = SystemDecorations.BorderOnly
        };

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        // Helper to create button
        Button CreateBtn(string text, DialogResult res, bool isDefault = false, bool isCancel = false)
        {
            var btn = new Button { Content = text, IsDefault = isDefault, IsCancel = isCancel };
            btn.Click += (_, _) => { result = res; window.Close(); };
            return btn;
        }

        // 3. Alt Action (mapped to Cancel usually, or a Neutral No)
        if (!string.IsNullOrEmpty(altText))
        {
            buttons.Children.Add(CreateBtn(altText, DialogResult.Cancel));
        }

        // 2. Negative Action (mapped to No)
        buttons.Children.Add(CreateBtn(cancelText, DialogResult.No, isCancel: true));

        // 1. Positive Action (mapped to Yes)
        buttons.Children.Add(CreateBtn(okText, DialogResult.Yes, isDefault: true));

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
                    buttons
                }
            }
        };

        window.Content = content;
        await window.ShowDialog(owner);
        return result;
    }
}