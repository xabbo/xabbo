using Microsoft.Extensions.Configuration;
using System;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AntiHcGiftNotificationComponent : Component
    {
        public AntiHcGiftNotificationComponent(IInterceptor interceptor,
            IConfiguration config)
            : base(interceptor)
        {
            IsActive = config.GetValue("AntiHcGiftNotification:Active", true);
        }

        [InterceptIn(nameof(Incoming.CSubscriptionUserGifts))]
        protected void HandleCSubscriptionUserGifts(InterceptArgs e)
        {
            if (IsActive)
                e.Block();
        }
    }
}
