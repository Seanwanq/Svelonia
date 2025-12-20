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
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="yesText"></param>
    /// <param name="noText"></param>
    /// <returns></returns>
    Task<bool> ShowConfirmAsync(string title, string message, string yesText = "yes", string noText = "No");
}