using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Xabbo.Messages;
using Xabbo.Extension;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;

namespace b7.Xabbo.Components;

public class AntiKickComponent : Component
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
        set => SetProperty(ref _isReady, value);
    }

    private bool _canReturnToPosition;
    public bool CanReturnToPosition
    {
        get => _canReturnToPosition;
        set => SetProperty(ref _canReturnToPosition, value);
    }

    private bool returnToPosition;
    public bool ReturnToPosition
    {
        get => returnToPosition;
        set => SetProperty(ref returnToPosition, value);
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

    protected override void OnInitialized(object? sender, ExtensionInitializedEventArgs e)
    {
        base.OnInitialized(sender, e);
    }

    private void OnEnteredRoom(object? sender, RoomEventArgs e)
    {
        _blockHotelView = false;
        IsReady = true;
    }

    private async Task HandleKickAsync(string msg)
    {
        _blockHotelView = true;
        _preventRoomRefresh = true;
        _lastKick = DateTime.Now;

        Extension.Send(Out.FlatOpc, (LegacyLong)_roomManager.CurrentRoomId, string.Empty, -1);
        _xabbot.ShowMessage(msg);

        if (_profileManager.UserData is not null)
        {
            if (_roomManager.Room is not null &&
                _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IRoomUser? self))
            {
                await Task.Delay(500);
                Extension.Send(Out.Move, self.X, self.Y);
            }
        }
    }

    [InterceptIn(nameof(Incoming.Notification))]
    public async void HandleNotification(InterceptArgs e)
    {
        if (!IsActive || _roomManager.CurrentRoomId <= 0) return;

        if (e.Packet.ReadString().Contains("room.kick.cannonball"))
        {
            e.Block();
            await HandleKickAsync("You were kicked by a cannon!");
        }
    }

    [InterceptIn(nameof(Incoming.Error))]
    public async void HandleError(InterceptArgs e)
    {
        if (!IsActive || _roomManager.CurrentRoomId <= 0)
            return;

        int errorCode = e.Packet.ReadInt();
        if (errorCode == ERROR_KICKED)
        {
            e.Block();
            await HandleKickAsync("You were kicked from the room!");
        }
    }

    [InterceptIn(nameof(Incoming.CloseConnection))]
    public void HandleCloseConnection(InterceptArgs e)
    {
        if (_blockHotelView)
        {
            _blockHotelView = false;
            if ((DateTime.Now - _lastKick).TotalSeconds < _rejoinThreshold)
                e.Block();
        }
    }

    [InterceptIn(nameof(Incoming.RoomEntryInfo))]
    public void HandleRoomEntryInfo(InterceptArgs e)
    {
        if (_preventRoomRefresh)
        {
            e.Block();
            _preventRoomRefresh = false;
        }
    }
}
