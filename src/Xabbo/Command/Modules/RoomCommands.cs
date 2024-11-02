using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Tasks;
using Xabbo.Core.Messages.Outgoing;
using Humanizer;

using In = Xabbo.Messages.Flash.In;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class RoomCommands(RoomManager roomManager) : CommandModule
{
    private readonly RoomManager _roomMgr = roomManager;

    [Command("clear", SupportedClients = ClientType.Modern)]
    public Task ClearCommandHandler(CommandArgs _)
    {
        if (_roomMgr.EnsureInRoom(out var room))
            Ext.Send(In.RoomEntryInfo, room.Id, _roomMgr.IsOwner);
        return Task.CompletedTask;
    }

    [Command("refresh", SupportedClients = ClientType.Modern)]
    public Task RefreshCommandHandler(CommandArgs _)
    {
        if (_roomMgr.IsInRoom)
            Ext.Send(Out.GetHeightMap);
        return Task.CompletedTask;
    }

    [Command("goto")]
    public Task GotoCommandHandler(CommandArgs args)
    {
        if (args.Length >= 1 && Id.TryParse(args[0], out Id roomId))
        {
            string password = "";
            if (args.Length >= 2)
                password = string.Join(" ", args.Skip(1));

            Task.Run(() => new EnterRoomTask(Ext, roomId, password).ExecuteAsync(3000));
        }
        else
        {
            ShowMessage("Usage: /goto <room id> [password]");
        }

        return Task.CompletedTask;
    }

    [Command("exit")]
    public Task ExitCommandHandler(CommandArgs args)
    {
        Ext.Send(Out.Quit);
        return Task.CompletedTask;
    }

    [Command("reload")]
    public Task ReloadCommandHandler(CommandArgs _)
    {
        if (_roomMgr.EnsureInRoom(out var room))
        {
            if (Session.Is(ClientType.Modern))
                Ext.Send(In.RoomForward, room.Id);
            else
                Ext.Send(Out.OpenFlatConnection, room.Id, "", (Id)(-1));
        }

        return Task.CompletedTask;
    }

    public async Task<bool> UpdateRoomSettingsAsync(Action<RoomSettings> update)
    {
        if (!_roomMgr.EnsureInRoom(out var room))
        {
            ShowMessage("Room state is not being tracked, please re-enter the room.");
            return false;
        }

        if (!_roomMgr.IsOwner)
        {
            ShowMessage("You must be the owner of the room to change room settings.");
            return false;
        }

        var settings = await Ext.RequestAsync(new GetRoomSettingsMsg(room.Id));
        update(settings);

        var receiver = Ext.ReceiveAsync(
            [In.RoomSettingsSaved, In.RoomSettingsSaveError],
            timeout: 2000,
            block: true
        );
        Ext.Send(Out.SaveRoomSettings, settings);

        var packet = await receiver;
        if (Ext.Messages.Is(packet.Header, In.RoomSettingsSaveError))
            throw new Exception("Server responded with an error when attempting to update room settings.");

        return true;
    }

    [Command("lock", SupportedClients = ClientType.Flash)]
    public async Task LockRoomAsync(CommandArgs args)
    {
        string? password = null;
        if (args.Length > 0)
        {
            password = string.Join(' ', args);
        }

        if (await UpdateRoomSettingsAsync(s =>
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
        }))
        {
            ShowMessage(string.IsNullOrWhiteSpace(password) ? "Room has been locked." : "Room password has been set.");
        }
    }

    [Command("open", SupportedClients = ClientType.Flash)]
    public async Task OpenRoomAsync(CommandArgs _)
    {
        if (await UpdateRoomSettingsAsync(s => s.Access = RoomAccess.Open))
            ShowMessage("Room has been opened.");
    }

    [Command("trading", SupportedClients = ClientType.Flash)]
    public async Task SetTradingAsync(CommandArgs args)
    {
        if (args is [ "on" or "off" or "rights", .. ])
        {
            var trading = args[0] switch
            {
                "on" => TradePermissions.Allowed,
                "rights" => TradePermissions.RightsHolders,
                _ => TradePermissions.NotAllowed
            };

            if (await UpdateRoomSettingsAsync(s => s.Trading = trading))
            {
                ShowMessage($"Trading permissions have been updated to: {trading.Humanize()}.");
            }
        }
    }

    [Command("access", "ra", Usage = "<open/hide/lock> [password]", SupportedClients = ClientType.Flash)]
    public async Task SetRoomAccessAsync(CommandArgs args)
    {
        if (args.Length < 1)
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
                    if (args.Length > 1)
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
