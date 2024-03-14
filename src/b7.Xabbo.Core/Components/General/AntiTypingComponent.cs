using System;

using Microsoft.Extensions.Configuration;

using Xabbo.Extension;
using Xabbo.Messages;

namespace b7.Xabbo.Components;

public class AntiTypingComponent : Component
{
    public AntiTypingComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
    {
        IsActive = config.GetValue("AntiTyping:Active", true);
    }

    [InterceptOut(nameof(Outgoing.UserStartTyping))]
    private void OnUserStartTyping(InterceptArgs e)
    {
        if (IsActive) e.Block();
    }
}
