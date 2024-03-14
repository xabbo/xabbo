using System;
using System.Threading.Tasks;

using Avalonia.Threading;

using b7.Xabbo.Services;

namespace b7.Xabbo.Avalonia.Services;

public class AvaloniaUiContext : IUiContext
{
    public bool IsSynchronized => Dispatcher.UIThread.CheckAccess();

    public void Invoke(Action callback)
    {
        Dispatcher.UIThread.Invoke(callback);
    }

    public Task InvokeAsync(Action callback)
    {
        return Dispatcher.UIThread.InvokeAsync(callback).GetTask();
    }
}
