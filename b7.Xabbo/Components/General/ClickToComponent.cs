using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Interceptor;
using Microsoft.Extensions.Configuration;

namespace b7.Xabbo.Components
{
    public class ClickToComponent : Component, IDataErrorInfo
    {
        private const string
            BAN_HOUR = "RWUAM_BAN_USER_HOUR",
            BAN_DAY = "RWUAM_BAN_USER_DAY",
            BAN_PERM = "RWUAM_BAN_USER_PERM";

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

        public ClickToComponent(IInterceptor interceptor,
            IConfiguration config,
            ProfileManager profileManager,
            FriendManager friendManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            _profileManager = profileManager;
            _friendManager = friendManager;
            _roomManager = roomManager;

            IsActive = config.GetValue<bool>("ClickTo:Active");

            switch (config.GetValue<string>("ClickTo:Mode")?.ToLower() ?? string.Empty)
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
            switch (config.GetValue("ClickTo:MuteUnit", "hours").ToLower())
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

            _disableForFriends = config.GetValue<bool>("ClickTo:DisableForFriends");

            _bounceUnbanDelay = config.GetValue("ClickTo:BounceUnbanDelay", 100);
            if (_bounceUnbanDelay < 0)
                _bounceUnbanDelay = 0;
        }

        [InterceptOut(nameof(Outgoing.GetSelectedBadges))]
        protected async void OnGetSelectedBadges(InterceptArgs e)
        {
            IRoom? room = _roomManager.Room;
            if (!IsActive || room is null)
                return;

            long userId = e.Packet.ReadLegacyLong();
            IRoomUser? user = room.GetEntityById<IRoomUser>(userId);
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
                Send(Out.RoomMuteUser, (LegacyLong)user.Id, (LegacyLong)_roomManager.CurrentRoomId, muteMinutes);
            }
            else if (Kick)
            {
                if (!_roomManager.CanKick)
                    return;

                SendInfoMessage("(click-kicking user)", user.Index);
                Send(Out.KickUser, (LegacyLong)user.Id);
            }
            else if (Ban)
            {
                if (!_roomManager.CanBan)
                    return;

                string banType, banText;

                if (BanDay)
                {
                    banType = BAN_DAY;
                    banText = "for a day";
                }
                else if (BanHour)
                {
                    banType = BAN_HOUR;
                    banText = "for an hour";
                }
                else if (BanPerm)
                {
                    banType = BAN_PERM;
                    banText = "permanently";
                }
                else
                    return;

                SendInfoMessage($"(click-banning user {banText})", user.Index);
                Send(Out.RoomBanWithDuration, (LegacyLong)user.Id, (LegacyLong)_roomManager.CurrentRoomId, banType);
            }
            else if (Bounce)
            {
                if (!_roomManager.IsOwner)
                    return;

                SendInfoMessage($"(click-bouncing user)", user.Index);
                Send(Out.RoomBanWithDuration, (LegacyLong)user.Id, (LegacyLong)_roomManager.CurrentRoomId, BAN_HOUR);
                await Task.Delay(_bounceUnbanDelay);
                Send(Out.RoomUnbanUser, (LegacyLong)user.Id, (LegacyLong)_roomManager.CurrentRoomId);
            }
        }

        private void SendInfoMessage(string message, int entityIndex = -1)
        {
            Send(In.Whisper, entityIndex, message, 0, 0, 0, 0);
        }
    }
}
