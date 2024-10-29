using System.Reactive.Linq;
using ReactiveUI;

using Xabbo.Messages.Flash;
using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Models;
using Xabbo.Core.Messages.Outgoing;

namespace Xabbo.Components;

public class LightingComponent : Component
{
    const string TonerIdentifier = "roombg_color";

    private readonly IGameDataManager _gameData;
    private readonly RoomManager _roomManager;

    private Id _currentBgTonerId = -1;

    private bool? _lastTonerActiveUpdate;
    private HslU8? _lastTonerColorUpdate;

    [Reactive] public bool IsTonerAvailable { get; set; }
    [Reactive] public bool IsTonerActive { get; set; }
    [Reactive] public HslU8 TonerColor { get; set; }

    public LightingComponent(IExtension ext, IGameDataManager gameData, RoomManager room)
        : base(ext)
    {
        _gameData = gameData;
        _roomManager = room;

        _gameData.Loaded += OnGameDataLoaded;
        _roomManager.Entered += OnEnteredRoom;
        _roomManager.Left += OnLeftRoom;
        _roomManager.FloorItemAdded += OnFloorItemAdded;
        _roomManager.FloorItemDataUpdated += OnFloorItemDataUpdated;

        this.WhenAnyValue(x => x.IsTonerActive)
            .Subscribe(EnableToner);

        this.WhenAnyValue(x => x.TonerColor)
            .Sample(TimeSpan.FromMilliseconds(500))
            .Subscribe(UpdateToner);
    }

    private void OnGameDataLoaded() => FindToner();

    private void FindToner()
    {
        if (!Extensions.IsInitialized)
            return;

        if (_roomManager.Room is not { } room)
            return;

        var toner = room.FloorItems.OfKind(TonerIdentifier).FirstOrDefault();
        if (toner is null)
        {
            IsTonerAvailable = false;
        }
        else
        {
            SetToner(toner);
        }
    }

    private void SetToner(IFloorItem toner)
    {
        _currentBgTonerId = toner.Id;
        if (toner.Data is IIntArrayData data && data.Count == 4)
        {
            _lastTonerActiveUpdate = data[0] != 0;
            IsTonerActive = _lastTonerActiveUpdate.Value;

            _lastTonerColorUpdate = new HslU8((byte)data[1], (byte)data[2], (byte)data[3]);
            TonerColor = _lastTonerColorUpdate.Value;

            IsTonerAvailable = true;
        }
    }

    private void UpdateToner(HslU8 color)
    {
        if (_lastTonerColorUpdate.Equals(color)) return;
        if (!Ext.IsConnected || _currentBgTonerId <= 0) return;
        _lastTonerColorUpdate = color;
        Ext.Send(Out.SetRoomBackgroundColorData, _currentBgTonerId, color);
    }

    private void EnableToner(bool enable)
    {
        if (_lastTonerActiveUpdate == enable ||
            _currentBgTonerId <= 0) return;
        _lastTonerActiveUpdate = enable;
        Ext.Send(new UseFloorItemMsg(_currentBgTonerId));
    }

    private void OnEnteredRoom(RoomEventArgs args)
    {
        FindToner();
    }

    private void OnLeftRoom()
    {
        _currentBgTonerId = -1;
        _lastTonerActiveUpdate = null;
        _lastTonerColorUpdate = null;

        IsTonerAvailable = false;
        IsTonerActive = false;
        TonerColor = default;
    }

    private void OnFloorItemAdded(FloorItemEventArgs e)
    {
        if (!Extensions.IsInitialized) return;

        if (e.Item.TryGetIdentifier(out string? identifier) &&
            identifier == TonerIdentifier)
        {
            _currentBgTonerId = e.Item.Id;
        }
    }

    private void OnFloorItemDataUpdated(FloorItemDataUpdatedEventArgs e)
    {
        if (e.Item.Id == _currentBgTonerId)
            SetToner(e.Item);
    }
}
