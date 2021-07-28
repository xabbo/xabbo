using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AntiKickComponent : Component
    {
        private const int ERROR_KICKED = 4008;

        private long _currentRoomId = -1;
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

        private ProfileManager _profileManager;
        private RoomManager _roomManager;

        public AntiKickComponent(IInterceptor interceptor,
            ProfileManager profileManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            _profileManager = profileManager;
            _roomManager = roomManager;

            _roomManager.Entered += OnEnteredRoom;
        }

        protected override void OnInitialized(object? sender, InterceptorInitializedEventArgs e)
        {
            base.OnInitialized(sender, e);
        }

        private void OnEnteredRoom(object? sender, RoomEventArgs e)
        {
            _currentRoomId = e.Room.Id;
            _blockHotelView = false;
            IsReady = true;
        }

        [InterceptIn(nameof(Incoming.Error))]
        public async void InGenericErrorMessages(InterceptArgs e)
        {
            if (!IsActive)
                return;

            long roomId = _roomManager.CurrentRoomId;
            if (roomId < 0)
                return;

            int errorCode = e.Packet.ReadInt();
            if (errorCode == ERROR_KICKED)
            {
                e.Block();
                _blockHotelView = true;
                _preventRoomRefresh = true;
                _lastKick = DateTime.Now;

                Send(Out.FlatOpc, roomId, string.Empty, -1);
                SendInfoMessage("You were kicked from the room!");

                if (_profileManager.UserData is not null)
                {
                    if (_roomManager.Room is not null &&
                        _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IRoomUser? self))
                    {
                        await Task.Delay(500);
                        Send(Out.Move, self.X, self.Y);
                    }
                }
            }
        }

        [InterceptIn(nameof(Incoming.CloseConnection))]
        public void InHotelView(InterceptArgs e)
        {
            if (_blockHotelView)
            {
                _blockHotelView = false;
                if ((DateTime.Now - _lastKick).TotalSeconds < _rejoinThreshold)
                    e.Block();
            }
        }

        [InterceptIn(nameof(Incoming.RoomEntryInfo))]
        public void InRoomOwner(InterceptArgs e)
        {
            if (_preventRoomRefresh)
            {
                e.Block();
                _preventRoomRefresh = false;
            }
        }

        private void SendInfoMessage(string message)
        {
            Send(In.Whisper, -1, message, 0, 34, 0, 0);
        }
    }
}
