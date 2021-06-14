using System;
using System.Threading.Tasks;

using Xabbo.Messages;

namespace b7.Xabbo.Commands
{
    public class MiscCommands : CommandModule
    {
        [Command("mood"), RequiredOut(nameof(Outgoing.RoomDimmerChangeState), nameof(Outgoing.RoomDimmerEditPresets))]
        protected async Task HandleMoodCommand(CommandArgs args)
        {
            if (args.Count > 0)
            {
                switch (args[0].ToLower())
                {
                    case "settings": Send(Out.RoomDimmerEditPresets); break;
                    default: break;
                }
            }
            else
            {
                Send(Out.RoomDimmerChangeState);
            }
        }
    }
}
