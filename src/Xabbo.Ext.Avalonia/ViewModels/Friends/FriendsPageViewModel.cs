using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.Ext.Avalonia.ViewModels;

public class FriendsPageViewModel : PageViewModel
{
    public override string Header => "Friends";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.People };

    public FriendsPageViewModel() { }
}
