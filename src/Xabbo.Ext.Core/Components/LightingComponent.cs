using System.Reactive.Linq;

using ReactiveUI;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;

using Xabbo.Ext.Model;
using Xabbo.Interceptor;

namespace Xabbo.Ext.Components;

public class LightingComponent : Component
{
    const string TonerIdentifier = "roombg_color";

    private readonly IGameDataManager _gameData;
    private readonly RoomManager _roomManager;

    private Id _currentBgTonerId = -1;

    private bool? _lastTonerActiveUpdate;
    private HslU8? _lastTonerColorUpdate;

    [Reactive] public bool IsTonerAvailable { get; set; }

    [Reactive] public bool TonerActive { get; set; }
    [Reactive] public HslU8 TonerColor { get; set; }

    public LightingComponent(IExtension ext, IGameDataManager gameData, RoomManager room)
        : base(ext)
    {
        _gameData = gameData;
        _roomManager = room;

        _gameData.Loaded += OnGameDataLoaded;
        _roomManager.FloorItemAdded += OnFloorItemAdded;

        this.ObservableForProperty(x => x.TonerColor)
            .Sample(TimeSpan.FromMilliseconds(500))
            .Subscribe(x => UpdateToner(x.Value));
    }

    private IFloorItem? GetBgToner(long id)
    {
        IFloorItem? toner = null;
        if (Extensions.IsInitialized)
        {
            var room = _roomManager.Room;
            if (room is not null)
            {
                if (id > 0)
                {
                    toner = room.GetFloorItem(id);
                }
                else
                {
                    toner = room.FloorItems.OfKind(TonerIdentifier).FirstOrDefault();
                }
            }
        }
        return toner;
    }

    private void UpdateToner(HslU8 color)
    {
        if (_lastTonerColorUpdate.Equals(color)) return;
        if (!Ext.IsConnected || _currentBgTonerId <= 0) return;
        Ext.Send(Out.SetRoomBackgroundColorData, _currentBgTonerId, color);
    }

    private void OnGameDataLoaded()
    {
        var room = _roomManager.Room;
        if (room is null) return;

        var toner = room.FloorItems.OfKind(TonerIdentifier).FirstOrDefault();
        if (toner is null) return;

        _currentBgTonerId = toner.Id;
        if (toner.Data is IIntArrayData data && data.Count == 4)
        {
            _lastTonerActiveUpdate = data[0] != 0;
            TonerActive = _lastTonerActiveUpdate.Value;

            _lastTonerColorUpdate = new HslU8((byte)data[1], (byte)data[2], (byte)data[3]);
            TonerColor = _lastTonerColorUpdate.Value;

            this.RaisePropertyChanged(nameof(TonerActive));
            this.RaisePropertyChanged(nameof(TonerColor));
        }
    }

    private void OnFloorItemAdded(FloorItemEventArgs e)
    {
        if (!Extensions.IsInitialized) return;

        string identifier = e.Item.GetIdentifier();
        if (identifier == TonerIdentifier)
        {
            _currentBgTonerId = e.Item.Id;
        }
    }
}
