using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

using Xabbo.Services.Abstractions;
using Xabbo.ViewModels;
using Xabbo.Avalonia.Views;

namespace Xabbo.Avalonia.Services;

public sealed class AvaloniaAppManager(IApplicationLifetime lifetime, MainViewModel mainViewModel) : IApplicationManager
{
    private readonly IApplicationLifetime _lifetime = lifetime;
    private readonly MainViewModel _mainViewModel = mainViewModel;

    public void BringToFront()
    {
        if (_lifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.Invoke(() => {
                var window = (desktop.MainWindow ??= new MainWindow
                {
                    DataContext = Application.Current?.DataContext
                });
                window.Show();
                window.Activate();
            });
        }
    }

    public void FlashWindow() { }
}