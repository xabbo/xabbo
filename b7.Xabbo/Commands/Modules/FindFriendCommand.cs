using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xabbo.Messages;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Tasks;
using b7.Xabbo.Components;

namespace b7.Xabbo.Commands
{
    public class FindFriendCommand : CommandModule
    {
        private readonly FriendManager _friendManager;

        public FindFriendCommand(FriendManager friendManager)
        {
            _friendManager = friendManager;
        }

        [Command("find", Usage = "<friendName>")]
        [RequiredOut(nameof(Outgoing.FollowFriend))]
        [RequiredIn(nameof(Incoming.RoomForward), nameof(Incoming.FollowFriendFailed))]
        protected async Task OnFind(CommandArgs args)
        {
            if (args.Count < 1)
                throw new InvalidArgsException();

            var friend = _friendManager.Friends
                .Where(x => x.Name.ToLower().Contains(args[0]))
                .OrderBy(x => Math.Abs(x.Name.Length - args[0].Length))
                .FirstOrDefault();

            if (friend == null)
            {
                ShowMessage($"Unable to find friend {args[0]}");
                return;
            }

            var captureTask = ReceiveAsync(new[] { In.RoomForward, In.FollowFriendFailed }, timeout: 15000, block: true);
            Send(Out.FollowFriend, (LegacyLong)friend.Id);
            var packet = await captureTask;

            if (packet.Header == In.RoomForward)
            {
                int roomId = packet.ReadInt();
                var roomData = await new GetRoomDataTask(Interceptor, roomId)
                    .ExecuteAsync(5000, CancellationToken.None);
                ShowMessage($"{friend.Name} is in room '{roomData.Name}' by {roomData.OwnerName} (id:{roomData.Id}){(roomData.IsInvisible ? "*" : "")}");
            }
            else
            {
                var error = (FollowFriendError)packet.ReadInt();
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
}