using Microsoft.Extensions.Configuration;
using System;

using Xabbo.Extension;
using Xabbo.Messages;

namespace b7.Xabbo.Components;

public class AntiHcGiftNotificationComponent : Component
{
    public AntiHcGiftNotificationComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
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
