using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Core.Game;
using Xabbo.Core.Tasks;

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
    protected async Task GotoCommandHandler(CommandArgs args)
    {
        if (args.Count < 1 || !long.TryParse(args[0], out long roomId))
        {
            ShowMessage("Usage: /goto <room id> [password]");
            return;
        }

        string password = "";
        if (args.Count > 1)
            password = string.Join(" ", args.Skip(1));

        Send(Out.FlatOpc, (LegacyLong)roomId, password, -1L);
    }

    [Command("exit"), RequiredOut(nameof(Outgoing.FlatOpc))]
    protected async Task ExitCommandHandler(CommandArgs args) => Send(Out.FlatOpc, 0, "", -1L);

    [Command("reload"), RequiredIn(nameof(Incoming.RoomForward))]
    protected async Task ReloadCommandHandler(CommandArgs args)
    {
        if (roomManager.IsInRoom)
            Send(In.RoomForward, (LegacyLong)roomManager.CurrentRoomId);
    }

    [Command("trigger"), RequiredOut(nameof(Outgoing.GetRoomEntryData))]
    protected async Task TriggerEntryWiredCommandHandler(CommandArgs args) => Send(Out.GetRoomEntryData);
}
