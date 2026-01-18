namespace BarterItemsStacks.Web.Services;

public sealed class Debouncer : IDisposable
{
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public void Debounce(int delayMs, Func<Task> action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        _cts?.Cancel();
        _cts?.Dispose();

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _ = RunAsync(delayMs, action, token);
    }

    private static async Task RunAsync(int delayMs, Func<Task> action, CancellationToken token)
    {
        try
        {
            await Task.Delay(delayMs, token);
            if (!token.IsCancellationRequested)
                await action();
        }
        catch (TaskCanceledException) { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _disposed = true;
    }
}