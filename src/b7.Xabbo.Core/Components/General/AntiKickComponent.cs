using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Messages.Flash;

namespace b7.Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class AntiKickComponent : Component
{
    private const int ERROR_KICKED = 4008;

    private bool _blockHotelView = false;
    private bool _preventRoomRefresh = false;

    private DateTime _lastKick = DateTime.MinValue;
    private readonly double _rejoinThreshold = 5.0;

    private bool _isReady;
    public bool IsReady
    {
        get => _isReady;
        set => Set(ref _isReady, value);
    }

    private bool _canReturnToPosition;
    public bool CanReturnToPosition
    {
        get => _canReturnToPosition;
        set => Set(ref _canReturnToPosition, value);
    }

    private bool returnToPosition;
    public bool ReturnToPosition
    {
        get => returnToPosition;
        set => Set(ref returnToPosition, value);
    }

    private readonly XabbotComponent _xabbot;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    public AntiKickComponent(IExtension extension,
        IConfiguration config,
        XabbotComponent xabbot,
        ProfileManager profileManager,
        RoomManager roomManager)
        : base(extension)
    {
        _xabbot = xabbot;
        _profileManager = profileManager;
        _roomManager = roomManager;

        _roomManager.Entered += OnEnteredRoom;

        IsActive = config.GetValue("AntiKick:Active", true);
    }

    protected override void OnInitialized(InitializedArgs e)
    {
        base.OnInitialized(e);
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        _blockHotelView = false;
        IsReady = true;
    }

    private async Task HandleKickAsync(string msg)
    {
        _blockHotelView = true;
        _preventRoomRefresh = true;
        _lastKick = DateTime.Now;

        Ext.Send(Out.OpenFlatConnection, _roomManager.CurrentRoomId, string.Empty, -1);
        _xabbot.ShowMessage(msg);

        if (_profileManager.UserData is not null)
        {
            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IUser? self))
            {
                await Task.Delay(500);
                Ext.Send(Out.MoveAvatar, self.X, self.Y);
            }
        }
    }

    [InterceptIn(nameof(In.NotificationDialog))]
    public void HandleNotification(Intercept e)
    {
        if (!IsActive || _roomManager.CurrentRoomId <= 0) return;

        if (e.Packet.Read<string>().Contains("room.kick.cannonball"))
        {
            e.Block();
            Task.Run(() => HandleKickAsync("You were kicked by a cannon!"));
        }
    }

    [InterceptIn(nameof(In.ErrorReport))]
    public void HandleErrorReport(Intercept e)
    {
        if (!IsActive || _roomManager.CurrentRoomId <= 0)
            return;

        int errorCode = e.Packet.Read<int>();
        if (errorCode == ERROR_KICKED)
        {
            e.Block();
            Task.Run(() => HandleKickAsync("You were kicked from the room!"));
        }
    }

    [InterceptIn(nameof(In.CloseConnection))]
    public void HandleCloseConnection(Intercept e)
    {
        if (_blockHotelView)
        {
            _blockHotelView = false;
            if ((DateTime.Now - _lastKick).TotalSeconds < _rejoinThreshold)
                e.Block();
        }
    }

    [InterceptIn(nameof(In.RoomEntryInfo))]
    public void HandleRoomEntryInfo(Intercept e)
    {
        if (_preventRoomRefresh)
        {
            e.Block();
            _preventRoomRefresh = false;
        }
    }
}
