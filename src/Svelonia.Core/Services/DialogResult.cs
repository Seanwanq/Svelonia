namespace Svelonia.Core.Services;

/// <summary>
/// Result of a dialog operation.
/// </summary>
public enum DialogResult
{
    /// <summary>
    /// The user closed the dialog without making a choice (e.g. via X button).
    /// </summary>
    None,

    /// <summary>
    /// The user clicked the positive action button (e.g. OK, Yes, Save).
    /// </summary>
    Ok,

    /// <summary>
    /// The user clicked the negative action button (e.g. Cancel, No).
    /// </summary>
    Cancel,

    /// <summary>
    /// The user clicked the Yes button.
    /// </summary>
    Yes,

    /// <summary>
    /// The user clicked the No button.
    /// </summary>
    No,
    
    /// <summary>
    /// The user clicked the Abort button.
    /// </summary>
    Abort,
    
    /// <summary>
    /// The user clicked the Retry button.
    /// </summary>
    Retry,
    
    /// <summary>
    /// The user clicked the Ignore button.
    /// </summary>
    Ignore
}
