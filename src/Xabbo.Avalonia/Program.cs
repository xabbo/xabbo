using System;
using ReactiveUI;
using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Media.Fonts;
using Avalonia.Media;

namespace Xabbo.Avalonia;

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
        .ConfigureFonts(fm => {
            fm.AddFontCollection(new EmbeddedFontCollection(
                new Uri("fonts:"),
                new Uri("avares://Xabbo.Avalonia/Assets/Fonts")));
        })
        .With(new FontManagerOptions
        {
            DefaultFamilyName = "fonts:#IBM Plex Sans",
            FontFallbacks = [ new FontFallback { FontFamily = "fonts:#Noto Emoji" } ]
        })
        .UseReactiveUI();
}
