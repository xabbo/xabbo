using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Command.Modules;

[CommandModule]
public sealed class TurnCommand(ProfileManager profileManager, RoomManager roomManager) : CommandModule
{
    private readonly ProfileManager _profileManager = profileManager;
    private readonly RoomManager _roomManager = roomManager;

    [Command("turn", "t")]
    private async Task HandleLookCommand(CommandArgs args)
    {
        if (args.Length == 0) return;

        int dir = -1;
        switch (args[0].ToLower())
        {
            case "n": dir = 0; break;
            case "ne": dir = 1; break;
            case "e": dir = 2; break;
            case "se": dir = 3; break;
            case "s": dir = 4; break;
            case "sw": dir = 5; break;
            case "w": dir = 6; break;
            case "nw": dir = 7; break;
            default: break;
        }

        if (dir > -1)
        {
            bool sendInverse = true;

            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetUserById(_profileManager.UserData?.Id ?? -1, out IUser? user))
            {
                if (user.Direction == dir)
                    return;
                // Server doesn't let you turn 45 degrees so we need to face
                // the opposite way first if we're only turning 45 degrees
                int phi = Math.Abs(user.Direction - dir) % 8;
                sendInverse = (phi > 4 ? (8 - phi) : phi) <= 1; // angle difference <= 45 degrees
            }

            int x, y;
            if (sendInverse)
            {
                (x, y) = H.GetMagicVector((dir + 4) % 8);
                Ext.Send(new LookToMsg(x, y));
                await Task.Delay(100);
            }
            (x, y) = H.GetMagicVector(dir);
            Ext.Send(new LookToMsg(x, y));
        }
    }
}
