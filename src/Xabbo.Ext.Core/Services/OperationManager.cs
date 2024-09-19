using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Xabbo.Ext.Components;

namespace Xabbo.Ext.Services;

public class OperationManager : IOperationManager
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly XabbotComponent _xabbotComponent;

    private readonly object _sync = new();
    private CancellationTokenSource? _cts;

    public bool IsRunning => _cts is not null;
    public bool IsCancelling { get; private set; }

    public OperationManager(
        IHostApplicationLifetime lifetime,
        XabbotComponent xabbotComponent)
    {
        _lifetime = lifetime;
        _xabbotComponent = xabbotComponent;
    }

    public bool Cancel()
    {
        lock (_sync)
        {
            if (!IsCancelling && _cts is not null)
            {
                _cts.Cancel();
                return true;
            }

            return false;
        }
    }

    public async Task RunAsync(Func<CancellationToken, Task> task, bool command = false)
    {
        lock (_sync)
        {
            if (_cts is not null)
            {
                throw new InvalidOperationException("A task is already running.");
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping);
        }

        try
        {
            await task(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            if (command && !_lifetime.ApplicationStopping.IsCancellationRequested)
            {
                _xabbotComponent.ShowMessage("Operation canceled.");
            }
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
}
