using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.ViewModels;

public sealed class InventoryPageViewModel(InventoryViewModel inventory) : PageViewModel
{
    public override string Header => "Inventory";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Box };

    public InventoryViewModel Inventory { get; } = inventory;
}