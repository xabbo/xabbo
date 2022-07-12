using System;

using Microsoft.Extensions.Configuration;

using Xabbo.Common;
using Xabbo.Interceptor;
using Xabbo.Messages;

using Xabbo.Core.Game;

namespace b7.Xabbo.Components
{
    public class DoorbellComponent : Component
    {
        private readonly FriendManager _friendManager;

        private bool _acceptFriends;
        public bool AcceptFriends
        {
            get => _acceptFriends;
            set => Set(ref _acceptFriends, value);
        }

        public DoorbellComponent(
            IInterceptor interceptor,
            IConfiguration config,
            FriendManager friendManager)
            : base(interceptor)
        {
            _friendManager = friendManager;

            AcceptFriends = config.GetValue("Doorbell:AcceptFriends", false);
        }

        [InterceptIn(nameof(Incoming.DoorbellRinging), RequiredClient = ClientType.Flash)]
        protected void HandleDoorbellRinging(InterceptArgs e)
        {
            if (Client != ClientType.Flash)
                return;

            string name = e.Packet.ReadString();
            if (AcceptFriends && _friendManager.IsFriend(name))
            {
                e.Block();
                Interceptor.Send(Out["LetUserIn"], name, true);
            }
        }
    }
}
