using Microsoft.Extensions.Logging;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core.Messages.Incoming;

namespace Xabbo.Ext.Components;

public class XabbotComponent : Component
{
    private readonly ILogger _logger;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    public long UserId { get; private set; } = 2_000_000_000;
    public int UserIndex { get; private set; } = 2_000_000_000;

    public XabbotComponent(
        ILoggerFactory logger,
        IExtension extension,
        ProfileManager profileManager, RoomManager roomManager)
        : base(extension)
    {
        _logger = logger.CreateLogger<XabbotComponent>();
        _profileManager = profileManager;
        _roomManager = roomManager;
        _roomManager.Entered += OnEnteredRoom;
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        Bot bot = new(AvatarType.PublicBot, UserId, UserIndex)
        {
            Name = "xabbo",
            Motto = "enhanced habbo",
            Location = new Tile(0, 0, -100),
            Gender = Gender.Male,
            Figure = "hr-100.hd-185-14.ch-805-71.lg-281-75.sh-305-80.ea-1406.cc-260-80"
        };

        _logger.LogTrace("Injecting xabbo avatar bot into room.");
        // Ext.Send(new AvatarsAddedMsg { bot });
    }

    public void ShowMessage(string message)
    {
        Point location = (0, 0);

        IUserData? userData = _profileManager.UserData;
        IRoom? room = _roomManager.Room;

        if (userData is not null && room is not null &&
            room.TryGetUserById(userData.Id, out IUser? user))
        {
            location = user.Location;
        }

        ShowMessage(message, location);
    }

    public void ShowMessage(string message, Point location)
    {
        // Ext.Send(new AvatarUpdatesMsg {
        //     new AvatarStatusUpdate {
        //         Index = UserIndex,
        //         Location = new Tile(location.X, location.Y, -100),
        //         Direction = 4,
        //         HeadDirection = 4
        //     }
        // });
        // Ext.Send(new AvatarWhisperMsg(UserIndex, message, 30));
    }
}
