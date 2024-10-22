using Splat;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Xabbo.Command;
using Xabbo.Controllers;
using Xabbo.Services;
using Xabbo.Services.Abstractions;
using Xabbo.Avalonia.Services;

using IHostApplicationLifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;

namespace Xabbo.Avalonia;

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

        // TODO A better way to initialize persistent background services.
        Locator.Current.GetRequiredService<IHostApplicationLifetime>();
        Locator.Current.GetRequiredService<IApplicationManager>();
        Locator.Current.GetRequiredService<IGameStateService>();
        Locator.Current.GetRequiredService<IFigureConverterService>();
        Locator.Current.GetRequiredService<CommandManager>();
        Locator.Current.GetRequiredService<ControllerInitializer>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        RxApp.DefaultExceptionHandler = Locator.Current.GetRequiredService<GlobalExceptionHandler>();

        base.OnFrameworkInitializationCompleted();
    }
}
