using System;
using System.Threading.Tasks;

using Splat;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Live.Avalonia;

using Xabbo.Ext.Avalonia.Services;
using Xabbo.Ext.Avalonia.Views;
using Xabbo.Ext.Commands;
using Xabbo.Ext.Core.Services;

namespace Xabbo.Ext.Avalonia;

public partial class App : Application, ILiveView
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
#if DEBUG
        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }
#endif

        var container = Locator.CurrentMutable;
        container.RegisterConstant<Application>(this);
        container.RegisterConstant(ApplicationLifetime);

        var mainViewModel = ViewModelLocator.Main;

        // Initialize persistent services
        Locator.Current.GetService<AppSessionManager>();
        Locator.Current.GetService<IGameStateService>();
        Locator.Current.GetService<IFigureConverterService>();
        Locator.Current.GetService<CommandManager>();

        // Initialize and run G-Earth extension lifetime
        if (Locator.Current.GetService<GEarthExtensionLifetime>() is not { } lifetime)
            throw new Exception($"Failed to obtain {nameof(GEarthExtensionLifetime)}.");
        Task.Run(lifetime.RunAsync);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            desktop.ShutdownRequested += (s, e) =>
            {
                desktop.MainWindow?.Close();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    public object CreateView(Window window) { throw new NotImplementedException(); }
}
