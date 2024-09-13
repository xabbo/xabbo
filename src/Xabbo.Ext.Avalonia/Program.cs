using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
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

    // [STAThread]
    // public static void Main(string[] args) => AppStarter.BuildAvaloniaApp<App>()
    //     .Start(AppMain, args);

    [STAThread]
    public static void Main(string[] args)
    {
        PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Avalonia);
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() => AppBuilder
        .Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        // .LogToTrace()
        .UseReactiveUI();
}
