using System;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;

using Xabbo.Messages;
using Xabbo.Interceptor;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;

namespace b7.Xabbo.ViewModel
{
    public class AlignerViewManager : ComponentViewModel
    {
        private RoomManager _roomManager;

        private bool haltAlignment;
        private WallLocation? originalLocation;

        private bool isInRoom;
        public bool IsInRoom
        {
            get => isInRoom;
            set => Set(ref isInRoom, value);
        }

        private bool isCapturing;
        public bool IsCapturing
        {
            get => isCapturing;
            set => Set(ref isCapturing, value);
        }

        private bool hasCapturedItem;
        public bool HasCapturedItem
        {
            get => hasCapturedItem;
            set
            {
                if (Set(ref hasCapturedItem, value))
                    RaisePropertyChanged(nameof(CanMoveLocation));
            }
        }

        private long currentId = -1;
        public long CurrentId
        {
            get => currentId;
            set => Set(ref currentId, value);
        }

        private string locationString = string.Empty;
        public string LocationString
        {
            get => locationString;
            set
            {
                if (!haltAlignment) WallLocation.Parse(value);
                Set(ref locationString, value);
            }
        }

        private int wallX;
        public int WallX
        {
            get => wallX;
            set => Set(ref wallX, value);
        }

        private int wallY;
        public int WallY
        {
            get => wallY;
            set => Set(ref wallY, value);
        }

        private int locationX;
        public int LocationX
        {
            get => locationX;
            set => Set(ref locationX, value);
        }

        private int locationY;
        public int LocationY
        {
            get => locationY;
            set => Set(ref locationY, value);
        }

        private bool isLeft = true;
        public bool IsLeft
        {
            get => isLeft;
            set => Set(ref isLeft, value);
        }

        private bool lockLocation;
        public bool LockLocation
        {
            get => lockLocation;
            set
            {
                if (Set(ref lockLocation, value))
                    RaisePropertyChanged(nameof(CanMoveLocation));
            }
        }

        public bool CanMoveLocation => HasCapturedItem && !LockLocation;

        public ICommand ToggleOrientationCommand { get; }
        public ICommand ResetCommand { get; }

        public AlignerViewManager(IInterceptor interceptor,
            RoomManager roomManager)
            : base(interceptor)
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

            bool isHaltingAlignment = haltAlignment;
            if (ignoreChangeEvents) return;

            IRoom? room = _roomManager.Room;

            try
            {
                haltAlignment = true;

                switch (propertyName)
                {
                    case "WallX":
                    case "WallY":
                    case "LocationX":
                    case "LocationY":
                    case "IsLeft":
                        int? scale = room?.FloorPlan.Scale;
                        if (LockLocation && scale.HasValue)
                        {
                            int diffX = WallX - previousWallX;
                            int diffY = WallY - previousWallY;

                            ignoreChangeEvents = true;
                            LocationX -= (diffX - diffY) * scale.Value / 2;
                            LocationY -= (diffX + diffY) * scale.Value / 4;
                            ignoreChangeEvents = false;
                        }

                        previousWallX = WallX;
                        previousWallY = wallY;

                        LocationString = WallLocation.ToString(WallX, WallY, LocationX, LocationY, IsLeft ? 'l' : 'r');
                        break;
                    case "LocationString":
                        if (WallLocation.TryParse(LocationString, out WallLocation location))
                        {
                            WallX = previousWallX = location.WallX;
                            WallY = previousWallY = location.WallY;
                            LocationX = location.X;
                            LocationY = location.Y;
                            IsLeft = location.Orientation.IsLeft;
                        }
                        break;
                    default: return;
                }
            }
            finally { haltAlignment = isHaltingAlignment; }

            if (haltAlignment) return;

            if (HasCapturedItem)
            {
                await SendAsync(Out.MoveWallItem, (LegacyLong)CurrentId, GetLocation().ToString());
            }
        }

        private WallLocation GetLocation() => new WallLocation(WallX, WallY, LocationX, LocationY, IsLeft ? 'l' : 'r');

        private void OnToggleOrientation()
        {
            IsLeft = !IsLeft;
        }

        private void Reset()
        {
            haltAlignment = true;

            try
            {
                HasCapturedItem = false;

                CurrentId = -1;
                LocationString = "";
                WallX = 0;
                WallY = 0;
                LocationX = 0;
                LocationY = 0;
                IsLeft = true;
            }
            finally { haltAlignment = false; }
        }

        private async void OnReset()
        {
            if (originalLocation is null) return;

            await SendAsync(Out.MoveWallItem, (LegacyLong)CurrentId, originalLocation.ToString());
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
                    haltAlignment = true;

                    HasCapturedItem = true;
                    CurrentId = itemId;
                    originalLocation = item.Location;
                    LocationString = item.Location.ToString();
                }
                finally { haltAlignment = false; }
            }
        }
    }
}
