using System.Reflection;
using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using Xabbo.Utility;

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
        Version = Assembly.GetEntryAssembly().GetVersionString();
        XabboCommonVersion = typeof(Xabbo.Client).Assembly.GetVersionString();
        XabboGEarthVersion = typeof(Xabbo.GEarth.GEarthExtension).Assembly.GetVersionString();
        XabboMessagesVersion = typeof(Xabbo.Messages.Flash.Out).Assembly.GetVersionString();
        XabboCoreVersion = typeof(Xabbo.Core.H).Assembly.GetVersionString();
    }
}
