using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace b7.Xabbo.Services;

public class DispatcherContext : IUiContext
{
    private Dispatcher _dispatcher;

    public DispatcherContext(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool IsSynchronized => _dispatcher.CheckAccess();
    public void Invoke(Action callback) => _dispatcher.Invoke(callback);
    public Task InvokeAsync(Action callback) => _dispatcher.InvokeAsync(callback).Task;
}
