using ReactiveUI;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

public class RoomModeratorComponent : Component
{
    private readonly RoomManager _roomManager;

    public bool HasRights => _roomManager.HasRights;
    public bool CanMute => _roomManager.CanMute;
    public bool CanKick => _roomManager.CanKick;
    public bool CanBan => _roomManager.CanBan;
    public bool IsOwner => _roomManager.IsOwner;

    public RoomModeratorComponent(IExtension extension, RoomManager roomManager)
        : base(extension)
    {
        _roomManager = roomManager;

        _roomManager.Entered += (e) => UpdatePermissions();
        _roomManager.RoomDataUpdated += (e) => UpdatePermissions();
        _roomManager.RightsUpdated += UpdatePermissions;
        _roomManager.Left += UpdatePermissions;
    }

    private void UpdatePermissions()
    {
        this.RaisePropertyChanged(nameof(HasRights));
        this.RaisePropertyChanged(nameof(CanMute));
        this.RaisePropertyChanged(nameof(CanKick));
        this.RaisePropertyChanged(nameof(CanBan));
        this.RaisePropertyChanged(nameof(IsOwner));
    }

    // [RequiredOut(nameof(Out.RoomMuteUser))]
    public bool MuteUser(Avatar e, int minutes)
    {
        if (!_roomManager.EnsureInRoom(out var room) ||
            _roomManager.CanMute)
            return false;

        Ext.Send(new MuteUserMsg(e.Id, room.Id, minutes));
        return true;
    }

    // [RequiredOut(nameof(Out.KickUser))]
    public bool KickUser(Avatar user)
    {
        if (!_roomManager.CanKick)
            return false;

        Ext.Send(new KickUserMsg
        {
            Id = user.Id,
            Name = user.Name,
        });
        return true;
    }

    // [RequiredOut(nameof(Out.RoomBanWithDuration))]
    public bool BanUser(Avatar user, BanDuration duration)
    {
        if (!_roomManager.EnsureInRoom(out var room) || !_roomManager.CanBan)
            return false;
        Ext.Send(new BanUserMsg(user.Id, room.Id, user.Name, duration));
        return true;
    }

    // [RequiredOut(nameof(Out.RoomUnbanUser))]
    public bool UnbanUser(Avatar e)
    {
        if (!_roomManager.EnsureInRoom(out var room) || !_roomManager.IsOwner)
            return false;
        Ext.Send(Out.UnbanUserFromRoom, e.Id, room.Id);
        return true;
    }
}
