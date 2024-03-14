using System;
using System.Linq;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Core;
using Xabbo.Core.Game;

namespace b7.Xabbo.Commands;

public class SpoofChatCommands : CommandModule
{
    private readonly RoomManager _roomManager;

    public SpoofChatCommands(RoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    [Command("spoof", Usage = "<userName> <message>")]
    protected async Task OnSpoof(CommandArgs args)
    {
        if (args.Count < 2)
            throw new InvalidArgsException();

        string name = args[0];
        string message = string.Join(" ", args.Skip(1));

        if (_roomManager.Room is null ||
            !_roomManager.Room.TryGetEntityByName(name, out IEntity? entity))
            return;

        Header header;
        switch (args.ChatType)
        {
            case ChatType.Talk: header = In.Chat; break;
            case ChatType.Shout: header = In.Shout; break;
            case ChatType.Whisper: header = In.Whisper; break;
            default: return;
        }

        Send(header, entity.Index, message, 0, args.BubbleStyle, 0, 0);
    }
}
