using Xabbo;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Tasks;

namespace b7.Xabbo.Commands;

public class RoomCommands(RoomManager roomManager) : CommandModule
{
    private readonly RoomManager _roomMgr = roomManager;

    [Command("go", SupportedClients = ClientType.Flash)]
    // [RequiredIn(nameof(In.Navigator2SearchResultBlocks))]
    // [RequiredOut(
    //     nameof(Out.FlatOpc),
    //     nameof(Out.Navigator2Search)
    // )]
    private async Task GoCommandHandler(CommandArgs args)
    {
        string searchText = string.Join(" ", args);
        if (string.IsNullOrWhiteSpace(searchText)) return;

        await Task.Yield();

        try
        {
            var results = await new SearchNavigatorTask(Ext, "query", searchText).ExecuteAsync(10000, CancellationToken.None);
            var room = results.GetRooms().FirstOrDefault();
            if (room == null)
            {
                ShowMessage("/go: No rooms found!");
            }
            else
            {
                Ext.Send(Out.GetGuestRoom, room.Id, 0, 1);
            }
        }
        catch (OperationCanceledException)
        {
            ShowMessage("/go: Operation timed out!");
        }
    }

    [Command("goto", SupportedClients = ClientType.Flash)]
    // [RequiredOut(nameof(Out.FlatOpc))]
    protected Task GotoCommandHandler(CommandArgs args)
    {
        if (args.Count >= 1 && !long.TryParse(args[0], out long roomId))
        {
            string password = "";
            if (args.Count > 1)
                password = string.Join(" ", args.Skip(1));

            Ext.Send(Out.OpenFlatConnection, (Id)roomId, password, (Id)(-1));
        }
        else
        {
            ShowMessage("Usage: /goto <room id> [password]");
        }

        return Task.CompletedTask;
    }

    [Command("exit")]
    //[RequiredOut(nameof(Out.FlatOpc))]
    protected Task ExitCommandHandler(CommandArgs args)
    {
        Ext.Send(Out.OpenFlatConnection, (Id)0, "", (Id)(-1));
        return Task.CompletedTask;
    }

    [Command("reload", SupportedClients = ClientType.Flash)]
    // [RequiredIn(nameof(In.RoomForward))]
    protected Task ReloadCommandHandler(CommandArgs args)
    {
        if (_roomMgr.IsInRoom)
            Ext.Send(In.RoomForward, _roomMgr.CurrentRoomId);
        return Task.CompletedTask;
    }

    [Command("entry", SupportedClients = ClientType.Flash)]
    // RequiredOut(nameof(Out.GetRoomEntryData))]
    protected Task TriggerEntryWiredCommandHandler(CommandArgs args)
    {
        // TODO: Send(Out.GetRoomEntryData);
        return Task.CompletedTask;
    }

    protected async Task UpdateRoomSettingsAsync(Action<RoomSettings> update)
    {
        if (!_roomMgr.IsInRoom)
        {
            ShowMessage("Room state is not being tracked, please re-enter the room.");
            return;
        }

        if (!_roomMgr.IsOwner)
        {
            ShowMessage("You must be the owner of the room to change room settings.");
            return;
        }

        var settings = await new GetRoomSettingsTask(Ext, _roomMgr.CurrentRoomId)
            .ExecuteAsync(2000, CancellationToken.None);
        update(settings);

        var receiver = Ext.ReceiveAsync(
            [In.RoomSettingsSaved, In.RoomSettingsSaveError],
            timeout: 2000,
            block: true
        );
        Ext.Send(Out.SaveRoomSettings, settings);

        var packet = await receiver;
        if (Ext.Messages.Is(packet.Header, In.RoomSettingsSaveError))
            throw new Exception("Failed to update room settings.");
    }

    [Command("lock", SupportedClients = ClientType.Flash)]
    // [RequiredIn(nameof(In.RoomSettingsData))]
    // [RequiredOut(nameof(Out.GetRoomSettings), nameof(Out.SaveRoomSettings))]
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

    [Command("open", SupportedClients = ClientType.Flash)]
    // [RequiredIn(nameof(In.RoomSettingsData))]
    // [RequiredOut(nameof(Out.GetRoomSettings), nameof(Out.SaveRoomSettings))]
    protected async Task OpenRoomAsync(CommandArgs args)
    {
        await UpdateRoomSettingsAsync(s => s.Access = RoomAccess.Open);
        ShowMessage("Room has been opened.");
    }

    [Command("access", "ra", Usage = "<open/hide/lock> [password]", SupportedClients = ClientType.Flash)]
    // [RequiredIn(nameof(In.RoomSettingsData))]
    // [RequiredOut(nameof(Out.GetRoomSettings), nameof(Out.SaveRoomSettings))]
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
