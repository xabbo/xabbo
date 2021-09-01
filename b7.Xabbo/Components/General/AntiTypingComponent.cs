using System;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AntiTypingComponent : Component
    {
        public AntiTypingComponent(IInterceptor interceptor,
            IConfiguration config)
            : base(interceptor)
        {
            IsActive = config.GetValue("AntiTyping:Active", true);
        }

        [InterceptOut(nameof(Outgoing.UserStartTyping))]
        private void OnUserStartTyping(InterceptArgs e)
        {
            if (IsActive) e.Block();
        }
    }
}
