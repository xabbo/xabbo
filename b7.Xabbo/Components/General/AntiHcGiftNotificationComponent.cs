using System;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AntiHcGiftNotificationComponent : Component
    {
        public AntiHcGiftNotificationComponent(IInterceptor interceptor)
            : base(interceptor)
        {
            IsActive = true;
        }

        [InterceptIn(nameof(Incoming.CSubscriptionUserGifts))]
        protected void HandleCSubscriptionUserGifts(InterceptArgs e)
        {
            if (IsActive)
                e.Block();
        }
    }
}
