using Xabbo.Messages;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Messages.Outgoing;
using System.Reactive.Linq;

namespace Xabbo.Command.Modules;

[CommandModule(SupportedClients = ClientType.Modern)]
public sealed class FindFriendCommand(FriendManager friendManager) : CommandModule
{
    private readonly SemaphoreSlim _waiting = new(5);
    private readonly SemaphoreSlim _throttle = new(1, 1);
    private readonly FriendManager _friendManager = friendManager;

    [Command("find", Usage = "<friend name>")]
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
            .Where(x => x.Name.Contains(args[0], StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(x => Math.Abs(x.Name.Length - args[0].Length))
            .FirstOrDefault();

        if (friend == null)
        {
            ShowMessage($"Unable to find friend {args[0]}");
            return;
        }

        if (!_waiting.Wait(0))
        {
            ShowMessage("Too many requests, slow down.");
            return;
        }

        try
        {
            await _throttle.WaitAsync();
            var throttleInterval = Task.Delay(5500);
            try
            {
                Task<IPacket> receiver = Ext.ReceiveAsync([In.RoomForward, In.FollowFriendFailed], 2000, true);
                Ext.Send(Out.FollowFriend, friend.Id);
                var packet = await receiver;

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
            finally
            {
                await throttleInterval;
                _throttle.Release();
            }
        }
        finally { _waiting.Release(); }
    }
}