using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;
using IconSource = FluentAvalonia.UI.Controls.IconSource;

using Xabbo.Core.Game;

namespace Xabbo.ViewModels;

public class RoomPageViewModel(
    RoomManager roomManager,
    RoomInfoViewModel info,
    RoomAvatarsViewModel avatars,
    RoomVisitorsViewModel visitors,
    RoomBansViewModel bans,
    RoomFurniViewModel furni
)
    : PageViewModel
{
    public override string Header => "Room";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Home };

    public RoomManager RoomManager { get; } = roomManager;
    public RoomInfoViewModel Info { get; } = info;
    public RoomAvatarsViewModel Avatars { get; } = avatars;
    public RoomVisitorsViewModel Visitors { get; } = visitors;
    public RoomBansViewModel Bans { get; } = bans;
    public RoomFurniViewModel Furni { get; } = furni;
}
