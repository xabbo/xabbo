using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Core.Game;
using Xabbo.Core.Tasks;

namespace b7.Xabbo.Commands
{
    [RequiredOut(nameof(Outgoing.FlatOpc), nameof(Outgoing.GetRoomEntryData))] // RequestHeightmap
    public class RoomCommands : CommandModule
    {
        private readonly RoomManager roomManager;

        public RoomCommands(RoomManager roomManager)
        {
            this.roomManager = roomManager;
        }

        protected override void OnInitialize()
        {

        }

        [Command("go")]
        [RequiredIn(nameof(Incoming.Navigator2SearchResultBlocks))]
        [RequiredOut(
            nameof(Outgoing.FlatOpc),
            nameof(Outgoing.Navigator2Search) // nameof(Outgoing.RequestNewNavigatorRooms)
        )]
        private async Task GoCommandHandler(CommandArgs args)
        {
            string searchText = string.Join(" ", args);
            if (string.IsNullOrWhiteSpace(searchText)) return;

            await Task.Yield();

            try
            {
                var results = await new SearchNavigatorTask(Interceptor, "query", searchText).ExecuteAsync(10000, CancellationToken.None);
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
            if (args.Count < 1 || !int.TryParse(args[0], out int roomId))
            {
                ShowMessage("Usage: /goto <room id> [password]");
                return;
            }

            string password = "";
            if (args.Count > 1)
                password = args[1];

            Send(Out.FlatOpc, roomId, password, -1);
        }

        [Command("exit"), RequiredOut(nameof(Outgoing.FlatOpc))]
        protected async Task ExitCommandHandler(CommandArgs args) => Send(Out.FlatOpc, 0, "", -1);

        [Command("reload"), RequiredOut(nameof(Incoming.RoomForward))]
        protected async Task ReloadCommandHandler(CommandArgs args)
        {
            if (roomManager.IsInRoom)
                Send(In.RoomForward, roomManager.CurrentRoomId);
        }

        [Command("trigger"), RequiredOut(nameof(Outgoing.GetRoomEntryData))] // RequestHeightmap
        protected async Task TriggerEntryWiredCommandHandler(CommandArgs args) => Send(Out.GetRoomEntryData);
    }
}
