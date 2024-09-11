using IconSource = FluentAvalonia.UI.Controls.IconSource;

using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

namespace Xabbo.Ext.Avalonia.ViewModels;

public class GameDataPageViewModel(FurniDataViewModel furniData) : PageViewModel
{
    public override string Header => "Game data";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Database };

    public FurniDataViewModel FurniData { get; } = furniData;
}
