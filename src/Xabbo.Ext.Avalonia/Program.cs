using System;
using Avalonia;
using Avalonia.Controls;
using Xabbo.Ext.Avalonia.Views;

namespace Xabbo.Ext.Avalonia;

internal sealed class Program
{
    // [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SettingsPlaylistItem))]
    /*[STAThread]
    public static void Main(string[] args) => AppStarter.Start<App>(args,
        () => null!,// ViewModelLocator.SettingsProvider.Value,
        () => "" // ViewModelLocator.AppPathService.UnhandledExceptionLogPath
    );*/

    [STAThread]
    public static void Main(string[] args) => AppStarter.BuildAvaloniaApp<App>()
        .Start(AppMain, args);

    public static AppBuilder BuildAvaloniaApp() => AppStarter.BuildAvaloniaApp<App>();

    static void AppMain(Application app, string[] args)
    {
        var window = new MainWindow
        {
            DataContext = ViewModelLocator.Main
        };

        app.Run(window);
    }

    /*
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    // public static void Main(string[] args) => CreateHost(args)
        //.Start();


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    */
}
