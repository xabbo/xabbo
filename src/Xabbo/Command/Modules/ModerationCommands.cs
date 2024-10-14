using System.Collections.Concurrent;
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

    private readonly ConcurrentDictionary<string, int> _muteList = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, BanDuration> _banList = new(StringComparer.OrdinalIgnoreCase);

    protected override void OnInitialize()
    {
        _roomManager.Left += RoomManager_Left;
        _roomManager.AvatarsAdded += OnAvatarsAdded;

        IsAvailable = true;
    }

    private void MuteUser(IUser user, int minutes)
    {
        if (_roomManager.EnsureInRoom(out var room))
            Ext.Send(new MuteUserMsg(user, room.Id, minutes));
    }

    private void UnmuteUser(IUser user)
    {
        if (_roomManager.EnsureInRoom(out var room))
            Ext.Send(new MuteUserMsg(user, room.Id, 0));
    }

    private void KickUser(IUser user) => Ext.Send(new KickUserMsg(user));

    private void BanUser(IUser user, BanDuration duration)
    {
        if (_roomManager.EnsureInRoom(out var room))
            Ext.Send(new BanUserMsg(user, room.Id, duration));
    }

    private void UnbanUser(Id userId)
    {
        if (_roomManager.EnsureInRoom(out var room))
            Ext.Send(Out.UnbanUserFromRoom, userId, room.Id);
    }

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
                _banList.TryRemove(user.Name, out _);
            }
            else if (_muteList.TryGetValue(user.Name, out int muteDuration))
            {
                ShowMessage($"Muting user '{user.Name}'");
                await Task.Delay(100);
                MuteUser(user, muteDuration);
                _muteList.TryRemove(user.Name, out _);
            }
        });
    }

    [Command("mute", SupportedClients = ClientType.Modern)]
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

    [Command("unmute", SupportedClients = ClientType.Modern)]
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
}
