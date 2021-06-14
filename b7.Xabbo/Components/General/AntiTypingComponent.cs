using System;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AntiTypingComponent : Component
    {
        public AntiTypingComponent(IInterceptor interceptor)
            : base(interceptor)
        { }

        [InterceptOut(nameof(Outgoing.UserStartTyping))]
        private void OnUserStartTyping(InterceptArgs e)
        {
            if (IsActive) e.Block();
        }
    }
}
