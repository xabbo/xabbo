using System;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Interceptor;

namespace b7.Xabbo.ViewModel
{
    public class RoomInfoViewManager : ComponentViewModel
    {
        private readonly RoomManager _roomManager;

        private bool _isInRoom;
        public bool IsInRoom
        {
            get => _isInRoom;
            set => Set(ref _isInRoom, value);
        }

        private IRoomData? _data;
        public IRoomData? Data
        {
            get => _data;
            set => Set(ref _data, value);
        }

        public RoomInfoViewManager(IInterceptor interceptor,
            RoomManager roomManager)
            : base(interceptor)
        {
            _roomManager = roomManager;
            roomManager.Entered += RoomManager_Entered;
            roomManager.RoomDataUpdated += RoomManager_RoomDataUpdated;
            roomManager.Left += RoomManager_Left;
        }

        private void RoomManager_Entered(object? sender, EventArgs e)
        {
            Data = _roomManager.Data;
        }

        private void RoomManager_RoomDataUpdated(object? sender, RoomDataEventArgs e)
        {
            Data = e.Data;
            IsInRoom = true;
        }

        private void RoomManager_Left(object? sender, EventArgs e)
        {
            IsInRoom = false;
            Data = null;
        }
    }
}
