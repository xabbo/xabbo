using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using Xabbo.Ext.Components;

using IconSource = FluentAvalonia.UI.Controls.IconSource;

namespace Xabbo.Ext.Avalonia.ViewModels;

public class RoomPageViewModel(
    RoomInfoViewModel info,
    RoomAvatarsViewModel avatars,
    RoomVisitorsViewModel visitors,
    RoomBansViewModel bans,
    RoomFurniViewModel furni) : PageViewModel
{
    public override string Header => "Room";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.Home };

    public RoomInfoViewModel Info { get; } = info;
    public RoomAvatarsViewModel Avatars { get; } = avatars;
    public RoomVisitorsViewModel Visitors { get; } = visitors;
    public RoomBansViewModel Bans { get; } = bans;
    public RoomFurniViewModel Furni { get; } = furni;
}
