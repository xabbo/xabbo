using FluentAvalonia.UI.Controls;
using Xabbo.Configuration;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

public sealed class SettingsPageViewModel(IConfigProvider<AppConfig> config) : PageViewModel
{
    public override string Header => "Settings";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Settings };

    private readonly IConfigProvider<AppConfig> _config = config;
    public AppConfig Config => _config.Value;
}
