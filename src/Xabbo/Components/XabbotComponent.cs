using Microsoft.Extensions.Logging;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

[Intercept]
public partial class XabbotComponent : Component
{
    private readonly ILogger _logger;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;
    private Point _currentLocation = (0, 0);

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

    [Intercept]
    void HandleLookTo(Intercept<LookToMsg> e)
    {
        if (e.Msg.Point == _currentLocation)
            e.Block();
    }

    [Intercept]
    void HandleKickUser(Intercept<KickUserMsg> e)
    {
        if (e.Msg.Id == UserId || e.Msg.Name == "xabbo")
        {
            e.Block();
            ShowMessage(":(");
        }
    }

    [Intercept]
    void HandleBanUser(Intercept<BanUserMsg> e)
    {
        if (e.Msg.Id == UserId || e.Msg.Name == "xabbo")
        {
            e.Block();
            ShowMessage(":(");
        }
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        Avatar avatar = Ext.Session.Client.Type switch
        {
            ClientType.Shockwave => new User(UserId, UserIndex) { Gender = Gender.Male },
            not ClientType.Shockwave => new Bot(AvatarType.PublicBot, UserId, UserIndex)
        };

        avatar.Name = "xabbo";
        avatar.Motto = "enhanced habbo";
        avatar.Location = new Tile(0, 0, -100);
        avatar.Figure = Ext.Session.Client.Type switch
        {
            ClientType.Shockwave => "1500225504295101800127534",
            not ClientType.Shockwave => "hr-100.hd-185-14.ch-805-71.lg-281-75.sh-305-80.ea-1406.cc-260-80"
        };

        _logger.LogTrace("Injecting xabbo avatar into room.");
        _currentLocation = avatar.Location;
        Ext.Send(new AvatarsAddedMsg { avatar });
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

    public void ShowMessage(string message, Point? location = null)
    {
        if (location.HasValue)
            _currentLocation = location.Value;

        Ext.Send(new AvatarStatusMsg {
            new AvatarStatus {
                Index = UserIndex,
                Location = new Tile(_currentLocation.X, _currentLocation.Y, -100),
                Direction = 4,
                HeadDirection = 4
            }
        });

        if (Session.Is(ClientType.Origins))
        {
            // Required on Origins to show the chat bubble at the correct location consistently.
            Task.Run(async () => {
                await Task.Delay(333);
                Ext.Send(new AvatarTalkMsg(message, UserIndex, 30));
            });
        }
        else
        {
            Ext.Send(new AvatarTalkMsg(message, UserIndex, 30));
        }
    }
}
