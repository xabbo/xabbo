using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

namespace b7.Xabbo.Commands
{
    public class ModerationCommands : CommandModule
    {
        private readonly RoomManager _roomManager;

        private readonly Dictionary<string, int> _muteList = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, BanDuration> _banList = new Dictionary<string, BanDuration>(StringComparer.OrdinalIgnoreCase);

        public ModerationCommands(RoomManager roomManager)
        {
            _roomManager = roomManager;
        }

        protected override void OnInitialize()
        {
            _roomManager.Left += RoomManager_Left;
            _roomManager.EntitiesAdded += OnEntitiesAdded;

            IsAvailable = true;
        }

        private void MuteUser(IRoomUser user, int minutes)
            => Send(Out.RoomMuteUser, (LegacyLong)user.Id, (LegacyLong)_roomManager.CurrentRoomId, minutes);

        private void UnmuteUser(IRoomUser user) => MuteUser(user, 0);

        private void KickUser(IRoomUser user) => Send(Out.KickUser, (LegacyLong)user.Id);

        private void BanUser(IRoomUser user, BanDuration duration)
            => Send(Out.RoomBanWithDuration, (LegacyLong)user.Id, (LegacyLong)_roomManager.CurrentRoomId, duration.GetValue());

        private void UnbanUser(int userId) => Send(Out.RoomUnbanUser, userId, _roomManager.CurrentRoomId);

        private void RoomManager_Left(object? sender, EventArgs e)
        {
            _muteList.Clear();
            _banList.Clear();
        }

        private async void OnEntitiesAdded(object? sender, EntitiesEventArgs e)
        {
            if (e.Entities.Length != 1) return;
            if (e.Entities[0] is not IRoomUser user) return;

            if (_banList.TryGetValue(user.Name, out BanDuration banDuration))
            {
                ShowMessage($"Banning user '{user.Name}'");
                await Task.Delay(100);
                BanUser(user, banDuration);
                _banList.Remove(user.Name);
            }
            else if (_muteList.TryGetValue(user.Name, out int muteDuration))
            {
                ShowMessage($"Muting user '{user.Name}'");
                await Task.Delay(100);
                MuteUser(user, muteDuration);
                _muteList.Remove(user.Name);
            }
        }

        [Command("mute"), RequiredOut(nameof(Outgoing.RoomMuteUser))]
        protected Task HandleMuteCommand(CommandArgs args)
        {
            if (args.Count < 2)
            {
                ShowMessage("/mute <name> <duration>[m|h]");
            }
            else if (!_roomManager.IsInRoom)
            {
                ShowMessage("Reload the room to initialize room state.");
            }
            else if (!_roomManager.CanMute)
            {
                ShowMessage("You do not have permission to mute in this room.");
            }
            else
            {
                string userName = args[0];

                if (_roomManager.Room is not null &&
                    _roomManager.Room.TryGetUserByName(userName, out IRoomUser? user))
                {
                    bool isHours = false;
                    string durationString = args[1];

                    if (durationString.EndsWith("m") || durationString.EndsWith("h"))
                    {
                        if (durationString.EndsWith("h"))
                            isHours = true;

                        durationString = durationString.Substring(0, durationString.Length - 1);
                    }

                    if (!int.TryParse(durationString, out int inputDuration) || inputDuration <= 0)
                    {
                        ShowMessage($"Invalid argument for duration: {args[1]}");
                    }
                    else
                    {
                        int duration = inputDuration;
                        if (isHours) duration *= 60;

                        if (duration > 30000)
                        {
                            ShowMessage($"Maximum mute time is 500 hours or 30,000 minutes.");
                        }
                        else
                        {
                            ShowMessage($"Muting user '{user.Name}' for {inputDuration} {(isHours ? "hour(s)" : "minute(s)")}");
                            MuteUser(user, duration);
                        }
                    }
                }
                else
                {
                    ShowMessage($"Unable to find user '{userName}' to mute.");
                }
            }
            return Task.CompletedTask;
        }

        [Command("unmute"), RequiredOut(nameof(Outgoing.RoomMuteUser))]
        protected async Task HandleUnmuteCommand(CommandArgs args)
        {
            if (args.Count < 1) return;

            if (!_roomManager.IsInRoom)
            {
                ShowMessage("Reload the room to initialize room state.");
                return;
            }

            if (!_roomManager.CanMute)
            {
                ShowMessage("You do not have permission to unmute in this room.");
                return;
            }

            string userName = args[0];

            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetEntityByName(userName, out IRoomUser? user))
            {
                ShowMessage($"Unmuting user '{user.Name}'");
                UnmuteUser(user);
            }
            else
            {
                ShowMessage($"Unable to find user '{userName}' to unmute.");
            }
        }

        [Command("kick"), RequiredOut(nameof(Outgoing.KickUser))]
        protected void HandleKickCommand(CommandArgs args)
        {
            if (args.Count < 1) return;

            if (!_roomManager.IsInRoom)
            {
                ShowMessage("Reload the room to initialize room state.");
                return;
            }

            if (!_roomManager.CanKick)
            {
                ShowMessage("You do not have permission to kick in this room.");
                return;
            }

            string userName = args[0];

            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetUserByName(userName, out IRoomUser? user))
            {
                ShowMessage($"Kicking user '{user.Name}'");
                KickUser(user);
            }
            else
            {
                ShowMessage($"Unable to find user '{userName}' to kick.");
            }
        }

        [Command("ban"), RequiredOut(nameof(Outgoing.RoomBanWithDuration))]
        protected async Task HandleBanCommand(CommandArgs args)
        {
            if (args.Count < 1) return;

            var banDuration = BanDuration.Hour;
            string durationString = "for an hour";

            if (args.Count > 1)
            {
                switch (args[1].ToLower())
                {
                    case "hour":
                        banDuration = BanDuration.Hour;
                        durationString = "for an hour";
                        break;
                    case "day":
                        banDuration = BanDuration.Day;
                        durationString = "for a day";
                        break;
                    case "perm":
                        banDuration = BanDuration.Permanent;
                        durationString = "permanently";
                        break;
                    default:
                        ShowMessage($"Unknown ban type '{args[1]}'.");
                        return;
                }
            }

            string userName = args[0];

            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetUserByName(userName, out IRoomUser? user))
            {
                ShowMessage($"Banning user '{user.Name}' {durationString}");
                BanUser(user, banDuration);
            }
            else
            {
                ShowMessage($"User '{userName}' not found, will be banned {durationString} upon next entry to this room.");
                _banList[userName] = banDuration;
            }
        }

        // [Command("unban"), RequiredOut(nameof(Outgoing.UnbanRoomUser))]
        protected async Task HandleUnbanCommand(CommandArgs args)
        {
            if (args.Count < 1) return;

            if (!_roomManager.IsInRoom)
            {
                ShowMessage("Reload the room to initialize room state.");
                return;
            }

            if (!_roomManager.IsOwner)
            {
                ShowMessage("You do not have permission to unban in this room.");
                return;
            }

            string userName = args[0];

            if (_banList.ContainsKey(userName))
            {
                _banList.Remove(userName);
                ShowMessage($"Removing user '{userName}' from the ban list");
            }
            else
            {
                // ...
            }
        }
    }
}
