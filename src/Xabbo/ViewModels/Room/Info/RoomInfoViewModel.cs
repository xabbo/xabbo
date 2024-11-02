using System.Reactive.Linq;
using ReactiveUI;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

namespace Xabbo.ViewModels;

public class RoomInfoViewModel : ViewModelBase
{
    private readonly RoomManager _roomManager;

    [Reactive] public bool IsInRoom { get; set; }
    [Reactive] public IRoomData? Data { get; set; }

    private readonly ObservableAsPropertyHelper<string?> _thumbnailUrl;
     public string? ThumbnailUrl => _thumbnailUrl.Value;

    private readonly ObservableAsPropertyHelper<bool> _isLoading;
    public bool IsLoading => _isLoading.Value;

    public RoomInfoViewModel(IExtension extension, RoomManager roomManager)
    {
        _roomManager = roomManager;

        extension.Disconnected += OnGameDisconnected;

        roomManager.Entered += OnEnteredRoom;
        roomManager.RoomDataUpdated += OnRoomDataUpdated;
        roomManager.Left += OnLeftRoom;

        _isLoading =
            roomManager.WhenAnyValue(
                x => x.IsInRoom,
                x => x.Room!.Data,
                (isInRoom, roomData) => isInRoom && roomData is null
            )
            .ToProperty(this, x => x.IsLoading);

        _thumbnailUrl =
            Observable.CombineLatest(
                _roomManager.WhenAnyValue(x => x.Room),
                extension.WhenAnyValue(x => x.Session),
                (room, session) => room is not null && session.Is(ClientType.Modern)
                    ? $"https://habbo-stories-content.s3.amazonaws.com/navigator-thumbnail/hh{session.Hotel.Identifier}/{room.Id}.png"
                    : null
            )
            .ToProperty(this, x => x.ThumbnailUrl);
    }

    private void OnEnteredRoom(RoomEventArgs e)
    {
        Data = e.Room.Data;
        IsInRoom = true;
    }

    private void OnRoomDataUpdated(RoomDataEventArgs e)
    {
        Data = e.Data;
        IsInRoom = true;
    }

    private void OnLeftRoom()
    {
        IsInRoom = false;
        Data = null;
    }

    private void OnGameDisconnected()
    {
        IsInRoom = false;
        Data = null;
    }
}
