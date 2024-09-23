using System.ComponentModel;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Core.Messages.Outgoing.Modern;

namespace Xabbo.Components;

[Intercept]
public partial class ClickToComponent(IExtension extension,
    IConfigProvider<AppConfig> config,
    ProfileManager profileManager,
    FriendManager friendManager,
    RoomManager roomManager) : Component(extension), IDataErrorInfo
{
    private readonly IConfigProvider<AppConfig> _config = config;
    private AppConfig Config => _config.Value;
    private readonly ProfileManager _profileManager = profileManager;
    private readonly FriendManager _friendManager = friendManager;
    private readonly RoomManager _roomManager = roomManager;

    [Reactive] public bool Enabled { get; set; }

    [Reactive] public bool Mute { get; set; }
    [Reactive] public int MuteValue { get; set; } = 1;
    [Reactive] public bool MuteInMinutes { get; set; }
    [Reactive] public bool MuteInHours { get; set; } = true;

    [Reactive] public bool Kick { get; set; }
    [Reactive] public bool Ban { get; set; }
    [Reactive] public bool BanHour { get; set; } = true;
    [Reactive] public bool BanDay { get; set; }
    [Reactive] public bool BanPerm { get; set; }

    [Reactive] public bool Bounce { get; set; }

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

    [Intercept(~ClientType.Shockwave)]
    [InterceptOut(nameof(Out.GetSelectedBadges))]
    protected void OnGetSelectedBadges(Intercept e)
    {
        IRoom? room = _roomManager.Room;
        if (!Enabled || room is null)
            return;

        Id userId = e.Packet.Read<Id>();
        IUser? user = room.GetAvatarById<IUser>(userId);
        if (user is null || user.Id == _profileManager.UserData?.Id) return;

        if (Config.General.ClickToIgnoresFriends && _friendManager.IsFriend(user.Id)) return;

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
                if (Config.General.BounceUnbanDelay > 0)
                    await Task.Delay(Config.General.BounceUnbanDelay);
                Ext.Send(Out.UnbanUserFromRoom, user.Id, _roomManager.CurrentRoomId);
            });
        }
    }

    private void SendInfoMessage(string message, int avatarIndex = -1) => Ext.Send(new AvatarWhisperMsg(avatarIndex, message));
}
