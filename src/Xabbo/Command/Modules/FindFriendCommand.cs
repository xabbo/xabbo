using Xabbo.Messages;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Command.Modules;

[CommandModule(SupportedClients = ~ClientType.Shockwave)]
public sealed class FindFriendCommand(FriendManager friendManager) : CommandModule
{
    private readonly FriendManager _friendManager = friendManager;

    [Command("find", Usage = "<friendName>")]
    // [RequiredOut(nameof(Out.FollowFriend))]
    // [RequiredIn(nameof(In.RoomForward), nameof(In.FollowFriendFailed))]
    public async Task OnFind(CommandArgs args)
    {
        if (args.Length < 1)
            throw new InvalidArgsException();

        if (!_friendManager.IsInitialized)
        {
            ShowMessage("Friend manager has not yet initialized.");
            return;
        }

        var friend = _friendManager.Friends
            .Where(x => x.Name.ToLower().Contains(args[0]))
            .OrderBy(x => Math.Abs(x.Name.Length - args[0].Length))
            .FirstOrDefault();

        if (friend == null)
        {
            ShowMessage($"Unable to find friend {args[0]}");
            return;
        }

        Task<IPacket> receiveAsync = Ext.ReceiveAsync([In.RoomForward, In.FollowFriendFailed], 2000, true);
        Ext.Send(Out.FollowFriend, friend.Id);
        var packet = await receiveAsync;

        if (Ext.Messages.Is(packet.Header, In.RoomForward))
        {
            int roomId = packet.Read<int>();
            var roomData = await Ext.RequestAsync(new GetRoomDataMsg(roomId));
            ShowMessage($"{friend.Name} is in room '{roomData.Name}' by {roomData.OwnerName} (id:{roomData.Id}){(roomData.IsInvisible ? "*" : "")}");
        }
        else
        {
            var error = (FollowFriendError)packet.Read<int>();
            switch (error)
            {
                case FollowFriendError.NotFriend:
                    ShowMessage($"{friend.Name} is not in your friend list");
                    break;
                case FollowFriendError.Offline:
                    ShowMessage($"{friend.Name} is offline");
                    break;
                case FollowFriendError.NotInRoom:
                    ShowMessage($"{friend.Name} is not in a room");
                    break;
                case FollowFriendError.CannotFollow:
                    ShowMessage($"{friend.Name} has follow disabled");
                    break;
                default:
                    ShowMessage($"Unknown error {error}");
                    break;
            }
        }
    }
}