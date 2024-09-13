using Avalonia.Controls.ApplicationLifetimes;

using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Avalonia.Services;

public sealed class AvaloniaAppManager : IApplicationManager
{
    private readonly IApplicationLifetime _lifetime;

    public AvaloniaAppManager(IApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public void BringToFront()
    {
        if (_lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Show();
        }
    }

    public void FlashWindow() { }
}