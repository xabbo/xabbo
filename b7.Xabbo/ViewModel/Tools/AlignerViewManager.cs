using System;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Extension;

namespace b7.Xabbo.ViewModel;

public class AlignerViewManager : ComponentViewModel
{
    private readonly RoomManager _roomManager;

    private bool _haltAlignment;
    private WallLocation? _originalLocation;

    private bool _isInRoom;
    public bool IsInRoom
    {
        get => _isInRoom;
        set => Set(ref _isInRoom, value);
    }

    private bool _isCapturing;
    public bool IsCapturing
    {
        get => _isCapturing;
        set => Set(ref _isCapturing, value);
    }

    private bool _hasCapturedItem;
    public bool HasCapturedItem
    {
        get => _hasCapturedItem;
        set
        {
            if (Set(ref _hasCapturedItem, value))
                RaisePropertyChanged(nameof(CanMoveLocation));
        }
    }

    private long _currentId = -1;
    public long CurrentId
    {
        get => _currentId;
        set => Set(ref _currentId, value);
    }

    private string _locationString = string.Empty;
    public string LocationString
    {
        get => _locationString;
        set
        {
            if (!_haltAlignment) WallLocation.Parse(value);
            Set(ref _locationString, value);
        }
    }

    private int _wx;
    public int WX
    {
        get => _wx;
        set => Set(ref _wx, value);
    }

    private int _wy;
    public int WY
    {
        get => _wy;
        set => Set(ref _wy, value);
    }

    private int _lx;
    public int LX
    {
        get => _lx;
        set => Set(ref _lx, value);
    }

    private int _ly;
    public int LY
    {
        get => _ly;
        set => Set(ref _ly, value);
    }

    private bool _isLeft = true;
    public bool IsLeft
    {
        get => _isLeft;
        set => Set(ref _isLeft, value);
    }

    private bool _lockLocation;
    public bool LockLocation
    {
        get => _lockLocation;
        set
        {
            if (Set(ref _lockLocation, value))
                RaisePropertyChanged(nameof(CanMoveLocation));
        }
    }

    public bool CanMoveLocation => HasCapturedItem && !LockLocation;

    public ICommand ToggleOrientationCommand { get; }
    public ICommand ResetCommand { get; }

    public AlignerViewManager(IExtension extension,
        RoomManager roomManager)
        : base(extension)
    {
        _roomManager = roomManager;

        _roomManager.Entered += RoomManager_Entered;
        _roomManager.Left += RoomManager_Left;

        _roomManager.WallItemRemoved += RoomManager_WallItemRemoved;

        ToggleOrientationCommand = new RelayCommand(OnToggleOrientation);
        ResetCommand = new RelayCommand(OnReset);
    }

    private bool ignoreChangeEvents = false;
    private int previousWallX, previousWallY;

    public override async void RaisePropertyChanged(string propertyName)
    {
        base.RaisePropertyChanged(propertyName);

        bool isHaltingAlignment = _haltAlignment;
        if (ignoreChangeEvents) return;

        IRoom? room = _roomManager.Room;

        try
        {
            _haltAlignment = true;

            switch (propertyName)
            {
                case nameof(WX):
                case nameof(WY):
                case nameof(LX):
                case nameof(LY):
                case nameof(IsLeft):
                    int? scale = room?.FloorPlan.Scale;
                    if (LockLocation && scale.HasValue)
                    {
                        int diffX = WX - previousWallX;
                        int diffY = WY - previousWallY;

                        ignoreChangeEvents = true;
                        LX -= (diffX - diffY) * scale.Value / 2;
                        LY -= (diffX + diffY) * scale.Value / 4;
                        ignoreChangeEvents = false;
                    }

                    previousWallX = WX;
                    previousWallY = _wy;

                    LocationString = WallLocation.ToString(WX, WY, LX, LY, IsLeft ? 'l' : 'r');
                    break;
                case nameof(LocationString):
                    if (WallLocation.TryParse(LocationString, out WallLocation location))
                    {
                        WX = previousWallX = location.WX;
                        WY = previousWallY = location.WY;
                        LX = location.LX;
                        LY = location.LY;
                        IsLeft = location.Orientation.IsLeft;
                    }
                    break;
                default: return;
            }
        }
        finally { _haltAlignment = isHaltingAlignment; }

        if (_haltAlignment) return;

        if (HasCapturedItem)
        {
            await Extension.SendAsync(Out.MoveWallItem, (LegacyLong)CurrentId, GetLocation().ToString());
        }
    }

    private WallLocation GetLocation() => new WallLocation(WX, WY, LX, LY, IsLeft ? 'l' : 'r');

    private void OnToggleOrientation()
    {
        IsLeft = !IsLeft;
    }

    private void Reset()
    {
        _haltAlignment = true;

        try
        {
            HasCapturedItem = false;

            CurrentId = -1;
            LocationString = "";
            WX = 0;
            WY = 0;
            LX = 0;
            LY = 0;
            IsLeft = true;
        }
        finally { _haltAlignment = false; }
    }

    private async void OnReset()
    {
        if (_originalLocation is null) return;

        await Extension.SendAsync(Out.MoveWallItem, (LegacyLong)CurrentId, _originalLocation.ToString());
    }

    private void RoomManager_Entered(object? sender, EventArgs e)
    {
        IsInRoom = true;
    }

    private void RoomManager_Left(object? sender, EventArgs e)
    {
        IsInRoom = false;
        Reset();
    }

    private void RoomManager_WallItemRemoved(object? sender, WallItemEventArgs e)
    {
        if (e.Item.Id == CurrentId)
        {
            Reset();
        }
    }

    [InterceptOut(nameof(Outgoing.MoveWallItem))]
    private void HandleMoveWallItem(InterceptArgs e)
    {
        if (IsCapturing)
        {
            e.Block();

            long itemId = e.Packet.ReadLegacyLong();
            IWallItem? item = _roomManager.Room?.GetWallItem(itemId);
            if (item is null) return;

            try
            {
                _haltAlignment = true;

                HasCapturedItem = true;
                CurrentId = itemId;
                _originalLocation = item.Location;
                LocationString = item.Location.ToString();
            }
            finally { _haltAlignment = false; }
        }
    }
}
