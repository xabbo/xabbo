using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class ModerationCommands(RoomManager roomManager) : CommandModule
{
    private readonly RoomManager _roomManager = roomManager;

    private readonly Dictionary<string, int> _muteList = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BanDuration> _banList = new(StringComparer.OrdinalIgnoreCase);

    protected override void OnInitialize()
    {
        _roomManager.Left += RoomManager_Left;
        _roomManager.AvatarsAdded += OnAvatarsAdded;

        IsAvailable = true;
    }

    private void MuteUser(IUser user, int minutes) => Ext.Send(new MuteUserMsg(user, _roomManager.CurrentRoomId, minutes));
    private void UnmuteUser(IUser user) => Ext.Send(new MuteUserMsg(user, _roomManager.CurrentRoomId, 0));
    private void KickUser(IUser user) => Ext.Send(new KickUserMsg(user));
    private void BanUser(IUser user, BanDuration duration) => Ext.Send(new BanUserMsg(user, _roomManager.CurrentRoomId, duration));
    private void UnbanUser(Id userId) => Ext.Send(Out.UnbanUserFromRoom, userId, _roomManager.CurrentRoomId);

    private void RoomManager_Left()
    {
        _muteList.Clear();
        _banList.Clear();
    }

    private void OnAvatarsAdded(AvatarsEventArgs e)
    {
        if (e.Avatars is not [User user]) return;

        Task.Run(async () =>
        {
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
        });
    }

    [Command("mute")]
    public Task HandleMuteCommand(CommandArgs args)
    {
        if (args.Length < 2)
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
                _roomManager.Room.TryGetUserByName(userName, out IUser? user))
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

    [Command("unmute")]
    public Task HandleUnmuteCommand(CommandArgs args)
    {
        if (args.Length < 1) return Task.CompletedTask;

        if (!_roomManager.IsInRoom)
        {
            ShowMessage("Reload the room to initialize room state.");
            return Task.CompletedTask;
        }

        if (!_roomManager.CanMute)
        {
            ShowMessage("You do not have permission to unmute in this room.");
            return Task.CompletedTask;
        }

        string userName = args[0];

        if (_roomManager.Room is not null &&
            _roomManager.Room.TryGetAvatarByName(userName, out IUser? user))
        {
            ShowMessage($"Unmuting user '{user.Name}'");
            UnmuteUser(user);
        }
        else
        {
            ShowMessage($"Unable to find user '{userName}' to unmute.");
        }
        return Task.CompletedTask;
    }

    [Command("kick")]
    public Task HandleKickCommand(CommandArgs args)
    {
        if (args.Length < 1) return Task.CompletedTask;

        if (!_roomManager.IsInRoom)
        {
            ShowMessage("Reload the room to initialize room state.");
            return Task.CompletedTask;
        }

        if (!_roomManager.CanKick)
        {
            ShowMessage("You do not have permission to kick in this room.");
            return Task.CompletedTask;
        }

        string userName = args[0];

        if (_roomManager.Room is not null &&
            _roomManager.Room.TryGetUserByName(userName, out IUser? user))
        {
            ShowMessage($"Kicking user '{user.Name}'");
            KickUser(user);
        }
        else
        {
            ShowMessage($"Unable to find user '{userName}' to kick.");
        }
        return Task.CompletedTask;
    }

    [Command("ban")]
    public Task HandleBanCommand(CommandArgs args)
    {
        if (args.Length < 1) return Task.CompletedTask;

        var banDuration = BanDuration.Hour;
        string durationString = "for an hour";

        if (args.Length > 1)
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
                    return Task.CompletedTask;
            }
        }

        string userName = args[0];

        if (_roomManager.Room is not null &&
            _roomManager.Room.TryGetUserByName(userName, out IUser? user))
        {
            ShowMessage($"Banning user '{user.Name}' {durationString}");
            BanUser(user, banDuration);
        }
        else
        {
            ShowMessage($"User '{userName}' not found, will be banned {durationString} upon next entry to this room.");
            _banList[userName] = banDuration;
        }
        return Task.CompletedTask;
    }

    // [Command("unban"), RequiredOut(nameof(Out.UnbanUser))]
    // public async Task HandleUnbanCommand(CommandArgs args)
    // {
    //     if (args.Count < 1) return;

    //     if (!_roomManager.IsInRoom)
    //     {
    //         ShowMessage("Reload the room to initialize room state.");
    //         return;
    //     }

    //     if (!_roomManager.IsOwner)
    //     {
    //         ShowMessage("You do not have permission to unban in this room.");
    //         return;
    //     }

    //     string userName = args[0];

    //     if (_banList.ContainsKey(userName))
    //     {
    //         _banList.Remove(userName);
    //         ShowMessage($"Removing user '{userName}' from the ban list");
    //     }
    //     else
    //     {
    //         // ...
    //     }
    // }
}
