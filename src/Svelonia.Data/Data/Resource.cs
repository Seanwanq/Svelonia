using Avalonia.Threading;

using Svelonia.Core;
namespace Svelonia.Data;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class Resource<T> : IDisposable
{
    private readonly IMediator _mediator;
    private readonly IRequest<T> _request;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// 
    /// </summary>
    public State<T?> Data { get; }

    /// <summary>
    /// 
    /// </summary>
    public State<bool> IsLoading { get; } = new(false);

    /// <summary>
    /// 
    /// </summary>
    public State<Exception?> Error { get; } = new(null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="request"></param>
    /// <param name="initialData"></param>
    public Resource(IMediator mediator, IRequest<T> request, T? initialData = default)
    {
        _mediator = mediator;
        _request = request;
        Data = new State<T?>(initialData);

        // Auto-fetch on creation? Yes, Svelonia resources usually auto-fetch.
        Refetch();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Refetch()
    {
        // Cancel previous
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsLoading.Value = true;
        Error.Value = null;

        Task.Run(async () =>
        {
            try
            {
                var result = await _mediator.Send(_request, token);
                Dispatcher.UIThread.Invoke(() =>
                {
                    Data.Value = result;
                    IsLoading.Value = false;
                });
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Error.Value = ex;
                    IsLoading.Value = false;
                });
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}