using IconSource = FluentAvalonia.UI.Controls.IconSource;

using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

namespace b7.Xabbo.Avalonia.ViewModels;

public class RoomPageViewModel(
    RoomInfoViewModel info,
    RoomEntitiesViewModel entities,
    RoomVisitorsViewModel visitors,
    RoomBansViewModel bans,
    RoomFurniViewModel furni) : PageViewModel
{
    public override string Header => "Room";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Home };

    public RoomInfoViewModel Info { get; } = info;
    public RoomEntitiesViewModel Entities { get; } = entities;
    public RoomVisitorsViewModel Visitors { get; } = visitors;
    public RoomBansViewModel Bans { get; } = bans;
    public RoomFurniViewModel Furni { get; } = furni;
}
