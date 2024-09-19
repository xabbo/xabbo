using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using ReactiveUI;

using Splat;

using Xabbo.Command;
using Xabbo.Services;
using Xabbo.Services.Abstractions;

using IHostApplicationLifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;

namespace Xabbo;

public partial class App : Application
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

        DataContext = ViewModelLocator.Main;

        // Initialize persistent services
        Locator.Current.GetRequiredService<IHostApplicationLifetime>();
        Locator.Current.GetRequiredService<AppSessionManager>();
        Locator.Current.GetRequiredService<GEarthExtensionLifetime>();
        Locator.Current.GetRequiredService<IGameStateService>();
        Locator.Current.GetRequiredService<IFigureConverterService>();
        Locator.Current.GetRequiredService<CommandManager>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        RxApp.DefaultExceptionHandler = Locator.Current.GetRequiredService<GlobalExceptionHandler>();

        base.OnFrameworkInitializationCompleted();
    }
}
