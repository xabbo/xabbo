using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

using Xabbo.Ext.Avalonia.ViewModels;
using Xabbo.Ext.Avalonia.Views;

using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Avalonia.Services;

public sealed class AvaloniaAppManager(IApplicationLifetime lifetime, MainViewModel mainViewModel) : IApplicationManager
{
    private readonly IApplicationLifetime _lifetime = lifetime;
    private readonly MainViewModel _mainViewModel = mainViewModel;

    public void BringToFront()
    {
        if (_lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.Invoke(() =>
                (desktop.MainWindow ??= new MainWindow
                {
                    DataContext = Application.Current?.DataContext
                }).Show()
            );
        }
    }

    public void FlashWindow() { }
}