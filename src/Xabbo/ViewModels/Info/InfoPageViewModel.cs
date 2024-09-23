using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;
using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.ViewModels;

public sealed class InfoPageViewModel : PageViewModel
{
    public override string Header => "Info";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Info };
}
