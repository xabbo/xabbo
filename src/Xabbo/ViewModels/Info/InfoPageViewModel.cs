using System.Reflection;
using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;
using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.ViewModels;

public sealed class InfoPageViewModel : PageViewModel
{
    public override string Header => "Info";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Info };

    public string Version { get; }
    public string XabboCommonVersion { get; }
    public string XabboGEarthVersion { get; }
    public string XabboMessagesVersion { get; }
    public string XabboCoreVersion { get; }

    public InfoPageViewModel()
    {
        Version = GetVersionString(Assembly.GetEntryAssembly());
        XabboCommonVersion = GetVersionString(typeof(Xabbo.Client).Assembly);
        XabboGEarthVersion = GetVersionString(typeof(Xabbo.GEarth.GEarthExtension).Assembly);
        XabboMessagesVersion = GetVersionString(typeof(Xabbo.Messages.Flash.Out).Assembly);
        XabboCoreVersion = GetVersionString(typeof(Xabbo.Core.H).Assembly);
    }

    static string GetVersionString(Assembly? assembly)
    {
        string? version = assembly?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly?.GetName().Version?.ToString(3);

        if (version is null)
            return "unknown version";

        if (!version.StartsWith('v'))
            version = "v" + version;

        int index = version.IndexOf('+');
        if (index > 0)
            version = version[..index];

        return version;
    }
}
