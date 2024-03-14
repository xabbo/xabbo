using System;

using ReactiveUI.Fody.Helpers;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

namespace b7.Xabbo.Avalonia.ViewModels;

public class RoomInfoViewModel : ViewModelBase
{
    private readonly RoomManager _roomManager;

    [Reactive] public bool IsInRoom { get; set; }
    [Reactive] public IRoomData? Data { get; set; }

    public RoomInfoViewModel(IExtension extension, RoomManager roomManager)
    {
        _roomManager = roomManager;

        extension.Disconnected += OnGameDisconnected;

        roomManager.Entered += OnEnteredRoom;
        roomManager.RoomDataUpdated += OnRoomDataUpdated;
        roomManager.Left += OnLeftRoom;
    }

    private void OnEnteredRoom(object? sender, EventArgs e)
    {
        Data = _roomManager.Data;
    }

    private void OnRoomDataUpdated(object? sender, RoomDataEventArgs e)
    {
        Data = e.Data;
        IsInRoom = true;
    }

    private void OnLeftRoom(object? sender, EventArgs e)
    {
        IsInRoom = false;
        Data = null;
    }

    private void OnGameDisconnected(object? sender, EventArgs e)
    {
        IsInRoom = false;
        Data = null;
    }
}
