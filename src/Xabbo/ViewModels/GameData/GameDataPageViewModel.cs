using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.ViewModels;

public class GameDataPageViewModel(
    FurniDataViewModel furniData,
    ExternalTextsViewModel texts,
    ExternalVariablesViewModel variables
)
    : PageViewModel
{
    public override string Header => "Game data";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Database };

    public FurniDataViewModel FurniData { get; } = furniData;
    public ExternalTextsViewModel Texts { get; } = texts;
    public ExternalVariablesViewModel Variables { get; } = variables;
}
