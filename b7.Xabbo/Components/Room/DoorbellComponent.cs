using System;

using Xabbo;
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

        public DoorbellComponent(IInterceptor interceptor,
            FriendManager friendManager)
            : base(interceptor)
        {
            _friendManager = friendManager;
        }

        [InterceptIn(nameof(Incoming.DoorbellRinging))]
        protected void HandleDoorbellRinging(InterceptArgs e)
        {
            if (Client != ClientType.Flash)
                return;

            string name = e.Packet.ReadString();
            if (AcceptFriends && _friendManager.IsFriend(name))
            {
                Send(Out["LetUserIn"], name, 1);
            }
        }
    }
}
