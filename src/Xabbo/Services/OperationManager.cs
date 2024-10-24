using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;

using Xabbo.Components;
using Xabbo.Exceptions;
using Xabbo.Services.Abstractions;

namespace Xabbo.Services;

public class OperationManager(IHostApplicationLifetime lifetime, XabbotComponent xabbotComponent) : IOperationManager
{
    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly XabbotComponent _xabbotComponent = xabbotComponent;

    private readonly object _sync = new();
    private CancellationTokenSource? _cts;
    private string _currentOperationName = "";

    public bool IsRunning => _cts is not null;
    public bool IsCancelling { get; private set; }

    public bool TryCancelOperation([NotNullWhen(true)] out string? operationName)
    {
        lock (_sync)
        {
            if (!IsCancelling && _cts is not null)
            {
                _cts.Cancel();
                operationName = _currentOperationName;
                return true;
            }

            operationName = null;
            return false;
        }
    }

    public async Task<T> RunAsync<T>(string operationName, Func<CancellationToken, Task<T>> task, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_cts is not null)
                throw new OperationInProgressException(_currentOperationName);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping, cancellationToken);
            _currentOperationName = operationName;
        }

        try
        {
            return await task(_cts.Token);
        }
        finally
        {
            lock (_sync)
            {
                IsCancelling = false;
                _cts.Dispose();
                _cts = null;
            }
        }
    }

    public async Task RunAsync(string operationName, Func<CancellationToken, Task> task, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_cts is not null)
                throw new OperationInProgressException(_currentOperationName);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping, cancellationToken);
            _currentOperationName = operationName;
        }

        try
        {
            await task(_cts.Token);
        }
        catch (OperationCanceledException) { }
        finally
        {
            lock (_sync)
            {
                IsCancelling = false;
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
