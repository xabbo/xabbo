using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Messages;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;

namespace b7.Xabbo.Components
{
    public class AntiTradeComponent : Component
    {
        private readonly IConfiguration _config;
        private readonly RoomManager _roomManager;
        private readonly ProfileManager _profileManager;
        private readonly TradeManager _tradeManager;

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set => Set(ref _isAvailable, value);
        }

        public AntiTradeComponent(
            IInterceptor interceptor,
            IConfiguration config,
            ProfileManager profileManager,
            RoomManager roomManager,
            TradeManager tradeManager)
            : base(interceptor)
        {
            _config = config;
            _profileManager = profileManager;
            _roomManager = roomManager;
            _tradeManager = tradeManager;

            _roomManager.Entered += OnRoomEntered;

            IsActive = _config.GetValue("AntiTrade:Active", false);

            Task initialization = Task.Run(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            await _profileManager.GetUserDataAsync();

            IsAvailable = true;
        }

        public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            UserData? userData = _profileManager.UserData;
            IRoom? room = _roomManager.Room;

            if (userData is null || room is null) return;

            if (propertyName == nameof(IsActive))
            {
                if (IsActive)
                {
                    if (_tradeManager.IsTrading)
                    {
                        IsActive = false;
                    }
                    else
                    {
                        if (room.TryGetUserById(userData.Id, out IRoomUser? self))
                        {
                            Send(Out.TradeOpen, self.Index);
                        }
                    }
                }
                else
                {
                    if (_tradeManager.IsTrading &&
                        _tradeManager.Partner is not null &&
                        _tradeManager.Partner.Id == userData.Id)
                    {
                        Send(Out.TradeClose);
                    }
                }
            }
        }

        [InterceptIn(nameof(Incoming.TradeOpen))]
        protected void HandleTradeOpen(InterceptArgs e)
        {
            if (IsActive)
            {
                e.Block();
            }
        }

        private void OnRoomEntered(object? sender, RoomEventArgs e)
        {
            UserData? userData = _profileManager.UserData;

            if (userData is null) return;

            if (IsActive)
            {
                if (e.Room.TryGetUserById(userData.Id, out IRoomUser? self))
                {
                    Send(Out.TradeOpen, self.Index);
                }
            }
        }
    }
}
