using Xabbo;
using Xabbo.Messages;
using Xabbo.Core.Game;
using Xabbo.Core.Tasks;
using Xabbo.Core;

namespace b7.Xabbo.Commands;

[RequiredOut(nameof(Outgoing.FlatOpc), nameof(Outgoing.GetRoomEntryData))]
public class RoomCommands : CommandModule
{
    private readonly RoomManager roomManager;

    public RoomCommands(RoomManager roomManager)
    {
        this.roomManager = roomManager;
    }

    [Command("go")]
    [RequiredIn(nameof(Incoming.Navigator2SearchResultBlocks))]
    [RequiredOut(
        nameof(Outgoing.FlatOpc),
        nameof(Outgoing.Navigator2Search)
    )]
    private async Task GoCommandHandler(CommandArgs args)
    {
        string searchText = string.Join(" ", args);
        if (string.IsNullOrWhiteSpace(searchText)) return;

        await Task.Yield();

        try
        {
            var results = await new SearchNavigatorTask(Extension, "query", searchText).ExecuteAsync(10000, CancellationToken.None);
            var room = results.GetRooms().FirstOrDefault();
            if (room == null)
            {
                ShowMessage("/go: No rooms found!");
            }
            else
            {
                Send(Out.GetGuestRoom, room.Id, 0, 1);
            }
        }
        catch (OperationCanceledException)
        {
            ShowMessage("/go: Operation timed out!");
        }
    }

    [Command("goto"), RequiredOut(nameof(Outgoing.FlatOpc))]
    protected Task GotoCommandHandler(CommandArgs args)
    {
        if (args.Count >= 1 && !long.TryParse(args[0], out long roomId))
        {
            string password = "";
            if (args.Count > 1)
                password = string.Join(" ", args.Skip(1));

            Send(Out.FlatOpc, (LegacyLong)roomId, password, -1L);
        }
        else
        {
            ShowMessage("Usage: /goto <room id> [password]");
        }

        return Task.CompletedTask;
    }

    [Command("exit"), RequiredOut(nameof(Outgoing.FlatOpc))]
    protected Task ExitCommandHandler(CommandArgs args)
    {
        Send(Out.FlatOpc, 0, "", -1L);
        return Task.CompletedTask;
    }

    [Command("reload"), RequiredIn(nameof(Incoming.RoomForward))]
    protected Task ReloadCommandHandler(CommandArgs args)
    {
        if (roomManager.IsInRoom)
            Send(In.RoomForward, (LegacyLong)roomManager.CurrentRoomId);
        return Task.CompletedTask;
    }

    [Command("entry"), RequiredOut(nameof(Outgoing.GetRoomEntryData))]
    protected Task TriggerEntryWiredCommandHandler(CommandArgs args)
    {
        Send(Out.GetRoomEntryData);
        return Task.CompletedTask;
    }

    protected async Task UpdateRoomSettingsAsync(Action<RoomSettings> update)
    {
        if (!roomManager.IsInRoom)
        {
            ShowMessage("Room state is not being tracked, please re-enter the room.");
            return;
        }

        if (!roomManager.IsOwner)
        {
            ShowMessage("You must be the owner of the room to change room settings.");
            return;
        }

        var settings = await new GetRoomSettingsTask(Extension, roomManager.CurrentRoomId)
            .ExecuteAsync(2000, CancellationToken.None);
        update(settings);

        var receiver = Extension.ReceiveAsync(
            (In.RoomSettingsSaved, In.RoomSettingsSaveError),
            timeout: 2000,
            block: true
        );
        Send(Out.SaveRoomSettings, settings);

        var packet = await receiver;
        if (packet.Header == In.RoomSettingsSaveError)
            throw new Exception("Failed to update room settings.");
    }

    [Command("lock")]
    [RequiredIn(nameof(Incoming.RoomSettingsData))]
    [RequiredOut(nameof(Outgoing.GetRoomSettings), nameof(Outgoing.SaveRoomSettings))]
    protected async Task LockRoomAsync(CommandArgs args)
    {
        string? password = null;
        if (args.Count > 0)
        {
            password = string.Join(' ', args);
        }

        await UpdateRoomSettingsAsync(s =>
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                s.Access = RoomAccess.Doorbell;
            }
            else
            {
                s.Password = password;
                s.Access = RoomAccess.Password;
            }
        });

    }

    [Command("open")]
    [RequiredIn(nameof(Incoming.RoomSettingsData))]
    [RequiredOut(nameof(Outgoing.GetRoomSettings), nameof(Outgoing.SaveRoomSettings))]
    protected async Task OpenRoomAsync(CommandArgs args)
    {
        await UpdateRoomSettingsAsync(s => s.Access = RoomAccess.Open);
        ShowMessage("Room has been opened.");
    }

    [Command("access", "ra", Usage = "<open/hide/lock> [password]")]
    [RequiredIn(nameof(Incoming.RoomSettingsData))]
    [RequiredOut(nameof(Outgoing.GetRoomSettings), nameof(Outgoing.SaveRoomSettings))]
    protected async Task SetRoomAccessAsync(CommandArgs args)
    {
        if (args.Count < 1)
            throw new InvalidArgsException();

        switch (args[0].ToUpperInvariant())
        {
            case "OPEN":
                {
                    await UpdateRoomSettingsAsync(s => s.Access = RoomAccess.Open);
                    ShowMessage("Room has been opened.");
                }
                break;
            case "HIDE":
                {
                    await UpdateRoomSettingsAsync(s => s.Access = RoomAccess.Invisible);
                    ShowMessage("Room is now hidden.");
                }
                break;
            case "LOCK":
                {
                    string? password = null;
                    if (args.Count > 1)
                        password = string.Join(' ', args.Skip(1));
                    await UpdateRoomSettingsAsync(s =>
                    {
                        if (string.IsNullOrWhiteSpace(password))
                        {
                            s.Access = RoomAccess.Doorbell;
                        }
                        else
                        {
                            s.Password = password;
                            s.Access = RoomAccess.Password;
                        }
                    });
                    ShowMessage(string.IsNullOrWhiteSpace(password) ? "Room password has been set." : "Room has been locked.");
                }
                break;
            default:
                throw new InvalidArgsException();
        }
    }
}
