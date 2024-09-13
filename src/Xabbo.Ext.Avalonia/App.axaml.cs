using System;
using System.Threading.Tasks;

using Splat;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

using Live.Avalonia;

using Xabbo.Ext.Avalonia.Services;
using Xabbo.Ext.Avalonia.Views;

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

        RequestedThemeVariant = ThemeVariant.Dark;

        // We must initialize the ViewModelLocator before setting GlobalErrorHandler.
        // We must set GlobalErrorHandler before View is created.

        // Set DefaultExceptionHelper now but we want to initialize ViewModelLocator later in parallel with View for faster startup.
        // GlobalErrorHandler.BeginInit();

        if (Locator.Current.GetService<GEarthExtensionLifetime>() is not { } lifetime)
            throw new Exception($"Failed to obtain {nameof(GEarthExtensionLifetime)}.");
        var tBackground = Task.Run(lifetime.RunAsync);

        // var dialogService = Locator.Current.GetService<IDialogService>()!;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // var settings = await AppStarter.AppSettingsLoader!.ConfigureAwait(true);
            // InitSettings(settings);

            // themeManager.RequestedTheme = settings.Theme.ToString();
            AppStarter.AppSettingsLoader = null;

            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            desktop.ShutdownRequested += (s, e) =>
            {
                desktop.MainWindow?.Close();
            };
        }

        // GlobalErrorHandler.EndInit(dialogService, desktop?.MainWindow?.DataContext as INotifyPropertyChanged);
        // RxApp.DefaultExceptionHandler = GlobalErrorHandler.Instance;

        base.OnFrameworkInitializationCompleted();
    }

    public object CreateView(Window window) { throw new NotImplementedException(); }
}
