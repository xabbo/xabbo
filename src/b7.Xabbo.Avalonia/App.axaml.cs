using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Splat;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using Live.Avalonia;

using b7.Xabbo.Avalonia.Services;

namespace b7.Xabbo.Avalonia;

public partial class App : Application, ILiveView
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected void BackgroundInit()
    {
        var lifetime = Locator.Current.GetService<GEarthExtensionLifetime>()!;
        Task.Run(() => lifetime.RunAsync());
    }

    public override async void OnFrameworkInitializationCompleted()
    {
#if DEBUG
        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }
#endif

        var vm = ViewModelLocator.Main;

        // We must initialize the ViewModelLocator before setting GlobalErrorHandler.
        // We must set GlobalErrorHandler before View is created.

        // Set DefaultExceptionHelper now but we want to initialize ViewModelLocator later in parallel with View for faster startup.
        // GlobalErrorHandler.BeginInit();

        var tBackground = Task.Run(BackgroundInit);
        // var dialogService = Locator.Current.GetService<IDialogService>()!;
        var themeManager = Locator.Current.GetService<FluentAvaloniaTheme>()!;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // var settings = await AppStarter.AppSettingsLoader!.ConfigureAwait(true);
            // InitSettings(settings);

            // themeManager.RequestedTheme = settings.Theme.ToString();
            AppStarter.AppSettingsLoader = null;

            // dialogService.Show(null, vm);
            // desktop.MainWindow = desktop.Windows[0];
            // desktop.MainWindow = new MainWindow();
        }

        // GlobalErrorHandler.EndInit(dialogService, desktop?.MainWindow?.DataContext as INotifyPropertyChanged);
        // RxApp.DefaultExceptionHandler = GlobalErrorHandler.Instance;

        try
        {
            await tBackground.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            // GlobalErrorHandler.ShowErrorLog(tBackground.Exception?.InnerException!);
        }
        base.OnFrameworkInitializationCompleted();
    }

    public object CreateView(Window window)
    {
        throw new NotImplementedException();
    }

    /*
    private IHost _host;
    public IServiceProvider Container { get; private set; } 

    public App() { }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        _host = Bootstrapper.CreateHost(Environment.GetCommandLineArgs());
        Container = _host.Services;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Container.GetRequiredService<MainWindow>();
            desktop.Exit += (s, e) =>
            {
                _host.StopAsync();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    */
}
