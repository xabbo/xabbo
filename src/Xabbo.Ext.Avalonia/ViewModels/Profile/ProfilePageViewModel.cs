using IconSource = FluentAvalonia.UI.Controls.IconSource;

using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

namespace Xabbo.Ext.Avalonia.ViewModels;

public class ProfilePageViewModel : PageViewModel
{
    public override string Header => "Profile";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Person };

    public ProfilePageViewModel() { }
}
