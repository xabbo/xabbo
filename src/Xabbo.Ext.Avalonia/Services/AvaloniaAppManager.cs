using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Avalonia.Services;

public sealed class AvaloniaAppManager(IApplicationLifetime lifetime) : IApplicationManager
{
    private readonly IApplicationLifetime _lifetime = lifetime;

    public void BringToFront()
    {
        if (_lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.Invoke(() => desktop.MainWindow?.Show());
        }
    }

    public void FlashWindow() { }
}