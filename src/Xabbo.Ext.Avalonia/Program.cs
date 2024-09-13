using System;

using Avalonia;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace Xabbo.Ext.Avalonia;

internal sealed class Program
{
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
