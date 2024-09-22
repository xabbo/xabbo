﻿using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

namespace Xabbo.ViewModels;

public class RoomInfoViewModel : ViewModelBase
{
    private readonly RoomManager _roomManager;

    [Reactive] public bool IsInRoom { get; set; }
    [Reactive] public IRoomData? Data { get; set; }

    private long _currentThumbnailId = -1;
    [Reactive] public string? ThumbnailUrl { get; set; }

    public RoomInfoViewModel(IExtension extension, RoomManager roomManager)
    {
        _roomManager = roomManager;

        extension.Disconnected += OnGameDisconnected;

        roomManager.Entered += OnEnteredRoom;
        roomManager.RoomDataUpdated += OnRoomDataUpdated;
        roomManager.Left += OnLeftRoom;
    }

    private void UpdateThumbnail(long roomId)
    {
        if (_currentThumbnailId == roomId)
            return;

        _currentThumbnailId = roomId;
        ThumbnailUrl = roomId > 0 ? $"https://habbo-stories-content.s3.amazonaws.com/navigator-thumbnail/hhus/{roomId}.png" : null;
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        Data = e.Room.Data;

        if (Data is not null)
        {
            UpdateThumbnail(Data.Id);
        }

        IsInRoom = true;
    }

    private void OnRoomDataUpdated(RoomDataEventArgs e)
    {
        Data = e.Data;
        IsInRoom = true;

        UpdateThumbnail(e.Data.Id);
    }

    private void OnLeftRoom()
    {
        UpdateThumbnail(0);
        IsInRoom = false;
        Data = null;
    }

    private void OnGameDisconnected()
    {
        IsInRoom = false;
        Data = null;
    }
}
