using Avalonia;
using Avalonia.Controls;
using Svelonia.Data;

using Svelonia.Core;
namespace Svelonia.Fluent;

/// <summary>
/// 
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    /// Executes a Command (Request) when the button is clicked. 
    /// Automatically disables the button while loading.
    /// </summary>
    public static T OnCommand<T>(this T control, ICommand command) where T : Button
    {
        return control.OnCommand(() => command);
    }

    /// <summary>
    /// Executes a Command created by the factory when the button is clicked.
    /// Use this when the command parameters depend on current UI state (e.g. TextBoxes).
    /// </summary>
    public static T OnCommand<T>(this T control, Func<ICommand> commandFactory) where T : Button
    {
        // We need to resolve the Mediator lazily
        IMediator? mediator = null;

        control.Click += async (s, e) =>
        {
            if (mediator == null)
            {
                if (Avalonia.Application.Current is Application app &&
                    app.GetType().GetProperty("Services")?.GetValue(app) is IServiceProvider sp)
                {
                    mediator = sp.GetService(typeof(IMediator)) as IMediator;
                }
            }

            if (mediator == null)
            {
                System.Diagnostics.Debug.WriteLine("Mediator not found for OnCommand.");
                return;
            }

            try
            {
                control.IsEnabled = false;
                var command = commandFactory(); // Create command with latest values
                await mediator.Send(command);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Command Execution Failed: {ex}");
            }
            finally
            {
                control.IsEnabled = true;
            }
        };

        return control;
    }

    /// <summary>
    /// Executes an Async Task when the button is clicked.
    /// Useful for binding to SveloniaForm.Submit or other async methods directly.
    /// </summary>
    public static T OnCommand<T>(this T control, Func<System.Threading.Tasks.Task> action) where T : Button
    {
        control.Click += async (s, e) =>
        {
            try
            {
                control.IsEnabled = false;
                await action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Command Execution Failed: {ex}");
            }
            finally
            {
                control.IsEnabled = true;
            }
        };
        return control;
    }
}
