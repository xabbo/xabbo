using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AntiIdleComponent : Component
    {
        private readonly IConfiguration _config;
        private readonly IHostApplicationLifetime _lifetime;

        private readonly ProfileManager _profileManager;
        private readonly RoomManager _roomManager;

        private int _latencyCheckCount = -1;

        private bool _isAntiIdleOutActive;
        public bool IsAntiIdleOutActive
        {
            get => _isAntiIdleOutActive;
            set => Set(ref _isAntiIdleOutActive, value);
        }

        public AntiIdleComponent(IInterceptor interceptor,
            IConfiguration config,
            IHostApplicationLifetime lifetime,
            ProfileManager profileManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            _config = config;
            _lifetime = lifetime;

            _profileManager = profileManager;
            _roomManager = roomManager;

            IsActive = config.GetValue("AntiIdle:Active", true);
            IsAntiIdleOutActive = config.GetValue("AntiIdle:AntiIdleOut", true);
        }

        protected override void OnDisconnected(object? sender, EventArgs e)
        {
            base.OnDisconnected(sender, e);

            _latencyCheckCount = -1;
        }

        public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(IsActive):
                case nameof(IsAntiIdleOutActive):
                    {
                        SendAntiIdlePacket();
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void OnInitialized(object? sender, InterceptorInitializedEventArgs e)
        {
            base.OnInitialized(sender, e);

            IsActive = _config.GetValue("AntiIdle:Active", true);
            IsAntiIdleOutActive = _config.GetValue("AntiIdle:IdleOutActive", true);
        }

        [InterceptIn(nameof(Incoming.ClientLatencyPingResponse))]
        protected void HandleClientLatencyPingResponse(InterceptArgs e)
        {
            _latencyCheckCount = e.Packet.ReadInt();

            if (_latencyCheckCount > 0 &&
                _latencyCheckCount % 12 == 0)
            {
                SendAntiIdlePacket();
            }
        }

        private void SendAntiIdlePacket()
        {
            if (_latencyCheckCount <= 0) return;

            if (_profileManager.UserData is not null &&
                _roomManager.Room is not null &&
                _roomManager.Room.TryGetUserById(_profileManager.UserData.Id, out IRoomUser? self))
            {
                if (self.IsIdle && IsAntiIdleOutActive)
                {
                    Interceptor.Send(Out.Expression, 5);
                }
                else if (self.Dance != 0 && IsActive)
                {
                    Interceptor.Send(Out.Move, 0, 0);
                }
                else if (IsActive)
                {
                    Interceptor.Send(Out.Expression, 0);
                }
            }
            else
            {
                if (IsActive)
                {
                    Interceptor.Send(Out.Expression, 0);
                }
            }
        }
    }
}
