using System;

using Humanizer;

using Xabbo.Core;
using Xabbo.Extension;
using Xabbo.Messages;

namespace b7.Xabbo.Components;

public class DisconnectionReasonComponent : Component
{
    public DisconnectionReasonComponent(IExtension extension)
        : base(extension)
    { }

    [InterceptIn(nameof(Incoming.DisconnectionReason))]
    protected void HandleDisconnectionReason(InterceptArgs e)
    {
        e.Block();

        DisconnectReason reason = (DisconnectReason)e.Packet.ReadInt();

        string reasonText = Enum.IsDefined(reason) ? reason.Humanize() : $"unknown ({(int)reason})";
        string message = $"[xabbo] You were disconnected by the server.\n\nReason: {reasonText}";

        Extension.Send(In.SystemBroadcast, message);
    }
}
