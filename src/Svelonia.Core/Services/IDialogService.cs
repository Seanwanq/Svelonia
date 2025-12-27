namespace Svelonia.Core.Services;

/// <summary>
/// 
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task ShowAlertAsync(string title, string message);

    /// <summary>
    /// Shows a confirmation dialog with configurable buttons.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The dialog message.</param>
    /// <param name="okText">Text for the positive button (returns DialogResult.Ok or Yes).</param>
    /// <param name="cancelText">Text for the negative button (returns DialogResult.Cancel or No). If null, this button might be hidden depending on implementation.</param>
    /// <param name="altText">Text for a third/neutral button (e.g. "Don't Save"). Returns DialogResult.No or None depending on mapping.</param>
    /// <returns>The result of the user's choice.</returns>
    Task<DialogResult> ShowConfirmAsync(string title, string message, string okText = "Yes", string cancelText = "No", string? altText = null);
}