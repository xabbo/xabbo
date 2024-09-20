using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.ViewModels;

public class ProfilePageViewModel : PageViewModel
{
    public override string Header => "Profile";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Person };

    public ProfilePageViewModel() { }
}
