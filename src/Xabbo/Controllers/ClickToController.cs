using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;
using Xabbo.Controllers;
using Humanizer;

namespace Xabbo.Components;

[Intercept]
public partial class ClickToController(
    IExtension extension,
    IConfigProvider<AppConfig> config,
    XabbotComponent xabbot,
    RoomModerationController moderation,
    ProfileManager profileManager,
    FriendManager friendManager,
    RoomManager roomManager)
:
    ControllerBase(extension)
{
    private readonly XabbotComponent _xabbot = xabbot;
    private readonly IConfigProvider<AppConfig> _config = config;
    private readonly RoomModerationController _moderation = moderation;
    private readonly ProfileManager _profileManager = profileManager;
    private readonly FriendManager _friendManager = friendManager;
    private readonly RoomManager _roomManager = roomManager;

    private AppConfig Config => _config.Value;

    [Reactive] public bool Enabled { get; set; }

    [Reactive] public bool Mute { get; set; } = true;
    [Reactive] public int MuteValue { get; set; } = 1;
    [Reactive] public bool MuteInMinutes { get; set; }
    [Reactive] public bool MuteInHours { get; set; } = true;

    [Reactive] public bool Kick { get; set; }
    [Reactive] public bool Ban { get; set; }
    [Reactive] public bool BanHour { get; set; } = true;
    [Reactive] public bool BanDay { get; set; }
    [Reactive] public bool BanPerm { get; set; }

    [Reactive] public bool Bounce { get; set; }

    [Intercept(~ClientType.Shockwave)]
    [InterceptOut(nameof(Out.GetSelectedBadges))]
    void OnGetSelectedBadges(Intercept e)
    {
        if (!Enabled || !_roomManager.EnsureInRoom(out var room))
            return;

        HandleClickUser(room.GetAvatarById<IUser>(e.Packet.Read<Id>()));
    }

    private bool CanClickTo(IUser user) =>
        user.Id != _profileManager.UserData?.Id &&
        user.Name != _profileManager.UserData?.Name &&
        (!Config.General.ClickToIgnoresFriends || !_friendManager.IsFriend(user.Name));

    // Use LookTo on Shockwave as it doesn't have GetSelectedBadges
    // TODO: allow [Intercept(ClientType.Shockwave)] in Xabbo.Common.Generator
    // to filter a message to certain client types
    [Intercept]
    void HandleLookTo(Intercept e, LookToMsg lookTo)
    {
        if (!Enabled || Session.Is(ClientType.Modern)) return;

        if (!_roomManager.EnsureInRoom(out IRoom? room))
            return;

        e.Block();

        var usersOnTile = room.Avatars
            .OfType<IUser>()
            .Where(user =>
                user.Location == lookTo.Point &&
                CanClickTo(user)
            )
            .ToArray();

        if (usersOnTile is [ IUser user ])
        {
            HandleClickUser(user);
        }
        else if (usersOnTile.Length > 1)
        {
            _xabbot.ShowMessage($"Cannot click-to: more than one user on tile.");
        }
    }

    private void HandleClickUser(IUser? user)
    {
        if (!Enabled) return;

        if (!_roomManager.EnsureInRoom(out IRoom? room))
            return;

        // Ignore self.
        if (user is null ||
            user.Id == _profileManager.UserData?.Id ||
            user.Name == _profileManager.UserData?.Name)
        {
            return;
        }

        // Ignore friends if configured.
        if (Config.General.ClickToIgnoresFriends && _friendManager.IsFriend(user.Id))
            return;

        if (Mute)
        {
            if (!_moderation.CanMute)
                return;

            int muteMinutes = MuteValue;
            if (MuteInHours)
                muteMinutes *= 60;

            if (muteMinutes < 0)
                muteMinutes = 0;
            if (muteMinutes > 30000)
                muteMinutes = 30000;

            SendInfoMessage($"(click-muting user for {(MuteInMinutes ? "minute" : "hour").ToQuantity(MuteValue)}", user.Index);
            Send(new MuteUserMsg(user.Id, room.Id, muteMinutes));
        }
        else if (Kick)
        {
            if (!_roomManager.CanKick)
                return;

            SendInfoMessage("(click-kicking user)", user.Index);
            Send(new KickUserMsg(user.Id, user.Name));
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
            Send(new BanUserMsg(user.Id, user.Name, room.Id, duration));
        }
        else if (Bounce)
        {
            if (!_roomManager.IsOwner)
                return;

            Task.Run(async () => {
                SendInfoMessage($"(click-bouncing user)", user.Index);
                Send(new BanUserMsg(user.Id, user.Name, room.Id, BanDuration.Hour));
                if (Config.General.BounceUnbanDelay > 0)
                    await Task.Delay(Config.General.BounceUnbanDelay);
                Send(Out.UnbanUserFromRoom, user.Id, room.Id);
            });
        }

    }

    private void SendInfoMessage(string message, int avatarIndex = -1) => Send(new AvatarWhisperMsg(message, avatarIndex));
}
