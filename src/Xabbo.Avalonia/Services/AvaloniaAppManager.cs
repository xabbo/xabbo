using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

using Xabbo.Services.Abstractions;
using Xabbo.ViewModels;
using Xabbo.Avalonia.Views;
using System;

namespace Xabbo.Avalonia.Services;

public sealed class AvaloniaAppManager(
    IApplicationLifetime lifetime,
    MainViewModel mainViewModel,
    Lazy<MainWindow> mainWindow) : IApplicationManager
{
    private readonly IApplicationLifetime _lifetime = lifetime;
    private readonly MainViewModel _mainViewModel = mainViewModel;
    private readonly Lazy<MainWindow> _mainWindow = mainWindow;

    public void BringToFront()
    {
        if (_lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.Invoke(() => {
                if (desktop.MainWindow is null)
                {
                    _mainWindow.Value.DataContext = Application.Current?.DataContext;
                    desktop.MainWindow = _mainWindow.Value;
                }
                _mainWindow.Value.Show();
                _mainWindow.Value.Activate();
            });
        }
    }

    public void FlashWindow() { }
}