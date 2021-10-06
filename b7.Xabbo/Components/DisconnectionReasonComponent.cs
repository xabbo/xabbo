using System;

using Humanizer;

using Xabbo.Core;
using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class DisconnectionReasonComponent : Component
    {
        public DisconnectionReasonComponent(IInterceptor interceptor)
            : base(interceptor)
        { }

        [InterceptIn(nameof(Incoming.DisconnectionReason))]
        protected void HandleDisconnectionReason(InterceptArgs e)
        {
            e.Block();

            DisconnectReason reason = (DisconnectReason)e.Packet.ReadInt();

            string reasonText = Enum.IsDefined(reason) ? reason.Humanize() : $"unknown ({(int)reason})";
            string message = $"[xabbo] You were disconnected by the server.\n\nReason: {reasonText}";

            Send(In.SystemBroadcast, message);
        }
    }
}
