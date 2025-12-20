using System.ComponentModel.DataAnnotations;
using Avalonia.Threading;

namespace Svelonia.Core.Data;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public class SveloniaForm<TResponse> : IDisposable
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 
    /// </summary>
    public State<bool> IsSubmitting { get; } = new(false);

    /// <summary>
    /// 
    /// </summary>
    public State<Exception?> Error { get; } = new(null);

    /// <summary>
    /// 
    /// </summary>
    public State<TResponse?> Result { get; } = new(default);

    /// <summary>
    /// 
    /// </summary>
    public event Action<TResponse>? OnSuccess;

    /// <summary>
    /// 
    /// </summary>
    public event Action<Exception>? OnFailure;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    public SveloniaForm(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    public async Task Submit(IRequest<TResponse> command)
    {
        IsSubmitting.Value = true;
        Error.Value = null;
        Result.Value = default;

        await Task.Run(async () =>
        {
            try
            {
                // 1. Client-side Validation (DataAnnotations)
                var validationContext = new ValidationContext(command);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(command, validationContext, validationResults, true))
                {
                    var firstError = validationResults.First().ErrorMessage ?? "Validation failed";
                    throw new ValidationException(firstError); // Simple single error for now
                    // TODO: Support field-level errors map
                }

                // 2.Server-side / Handler Execution
                var result = await _mediator.Send(command);

                Dispatcher.UIThread.Invoke(() =>
                {
                    Result.Value = result;
                    IsSubmitting.Value = false;
                    OnSuccess?.Invoke(result);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Error.Value = ex;
                    IsSubmitting.Value = false;
                    OnFailure?.Invoke(ex);
                });
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        // Cleanup if necessary
    }
}

/// <summary>
/// 
/// </summary>
public class SveloniaForm : SveloniaForm<Unit>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    public SveloniaForm(IMediator mediator) : base(mediator)
    { }
}