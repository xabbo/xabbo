using System.ComponentModel;

using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Core.Messages.Incoming;

namespace b7.Xabbo.Components;

[Intercept]
public partial class ClickToComponent : Component, IDataErrorInfo
{
    private readonly ProfileManager _profileManager;
    private readonly FriendManager _friendManager;
    private readonly RoomManager _roomManager;

    private bool mute;
    public bool Mute
    {
        get => mute;
        set => Set(ref mute, value);
    }

    public int muteValue = 500;
    public int MuteValue
    {
        get => muteValue;
        set => Set(ref muteValue, value);
    }

    private bool muteInMinutes;
    public bool MuteInMinutes
    {
        get => muteInMinutes;
        set => Set(ref muteInMinutes, value);
    }

    private bool muteInHours = true;
    public bool MuteInHours
    {
        get => muteInHours;
        set => Set(ref muteInHours, value);
    }

    private bool kick = true;
    public bool Kick
    {
        get => kick;
        set => Set(ref kick, value);
    }

    private bool ban;
    public bool Ban
    {
        get => ban;
        set => Set(ref ban, value);
    }

    private bool banHour;
    public bool BanHour
    {
        get => banHour;
        set => Set(ref banHour, value);
    }

    private bool banDay;
    public bool BanDay
    {
        get => banDay;
        set => Set(ref banDay, value);
    }

    private bool banPerm = true;
    public bool BanPerm
    {
        get => banPerm;
        set => Set(ref banPerm, value);
    }

    private bool bounce;
    public bool Bounce
    {
        get => bounce;
        set => Set(ref bounce, value);
    }

    public string Error => "...";

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(MuteValue):
                    // TODO
                    return string.Empty;
                default: return string.Empty;
            }
        }
    }

    private bool _disableForFriends;
    private int _bounceUnbanDelay;

    public ClickToComponent(IExtension extension,
        IConfiguration config,
        ProfileManager profileManager,
        FriendManager friendManager,
        RoomManager roomManager)
        : base(extension)
    {
        _profileManager = profileManager;
        _friendManager = friendManager;
        _roomManager = roomManager;

        IsActive = config.GetValue("ClickTo:Active", false);

        switch (config.GetValue("ClickTo:Mode", "kick")?.ToLower())
        {
            case "mute":
                Mute = true;
                break;
            case "kick":
                Kick = true;
                break;
            case "ban":
                Ban = true;
                break;
            case "bounce":
                Bounce = true;
                break;
            default:
                Kick = true;
                break;
        }

        BanHour = true;

        MuteValue = config.GetValue("ClickTo:MuteValue", 500);
        switch (config.GetValue("ClickTo:MuteUnit", "hours")?.ToLower())
        {
            case "m":
            case "mn":
            case "min":
            case "minutes":
                MuteInMinutes = true;
                break;
            default:
                MuteInHours = true;
                break;
        }

        _disableForFriends = config.GetValue("ClickTo:IgnoreFriends", true);

        _bounceUnbanDelay = config.GetValue("ClickTo:BounceUnbanDelay", 100);
        if (_bounceUnbanDelay < 0)
            _bounceUnbanDelay = 0;
    }

    [Intercept(~ClientType.Shockwave)]
    [InterceptOut(nameof(Out.GetSelectedBadges))]
    protected void OnGetSelectedBadges(Intercept e)
    {
        IRoom? room = _roomManager.Room;
        if (!IsActive || room is null)
            return;

        Id userId = e.Packet.Read<Id>();
        IUser? user = room.GetAvatarById<IUser>(userId);
        if (user is null || user.Id == _profileManager.UserData?.Id) return;

        if (_disableForFriends && _friendManager.IsFriend(user.Id)) return;

        if (Mute)
        {
            if (!_roomManager.CanMute)
                return;

            int muteMinutes = MuteValue;
            if (MuteInHours)
                muteMinutes *= 60;

            if (muteMinutes < 0)
                muteMinutes = 0;
            if (muteMinutes > 30000)
                muteMinutes = 30000;

            SendInfoMessage($"(click-muting user for {MuteValue} {(MuteInMinutes ? "minute(s)" : "hour(s)")})", user.Index);
            Ext.Send(new MuteUserMsg(user.Id, _roomManager.CurrentRoomId, muteMinutes));
        }
        else if (Kick)
        {
            if (!_roomManager.CanKick)
                return;

            SendInfoMessage("(click-kicking user)", user.Index);
            Ext.Send(new KickUserMsg(user.Id, user.Name));
        }
        else if (Ban)
        {
            if (!_roomManager.CanBan)
                return;

            BanDuration duration;
            string banText;

            if (BanDay)
            {
                duration = BanDuration.Day;
                banText = "for a day";
            }
            else if (BanHour)
            {
                duration = BanDuration.Hour;
                banText = "for an hour";
            }
            else if (BanPerm)
            {
                duration = BanDuration.Permanent;
                banText = "permanently";
            }
            else
                return;

            SendInfoMessage($"(click-banning user {banText})", user.Index);
            Ext.Send(new BanUserMsg(user.Id, _roomManager.CurrentRoomId, user.Name, duration));
        }
        else if (Bounce)
        {
            if (!_roomManager.IsOwner)
                return;

            Task.Run(async () => {
                SendInfoMessage($"(click-bouncing user)", user.Index);
                Ext.Send(new BanUserMsg(user.Id, _roomManager.CurrentRoomId, user.Name, BanDuration.Hour));
                await Task.Delay(_bounceUnbanDelay);
                Ext.Send(Out.UnbanUserFromRoom, user.Id, _roomManager.CurrentRoomId);
            });
        }
    }

    private void SendInfoMessage(string message, int avatarIndex = -1) => Ext.Send(new AvatarWhisperMsg(avatarIndex, message));
}
