using Microsoft.Extensions.Configuration;

using Xabbo;
using Xabbo.Extension;
using Xabbo.Messages.Flash;

namespace Xabbo.Ext.Components;

[Intercept(~ClientType.Shockwave)]
public partial class AntiHcGiftNotificationComponent : Component
{
    public AntiHcGiftNotificationComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
    {
        IsActive = config.GetValue("AntiHcGiftNotification:Active", true);
    }

    [InterceptIn(nameof(In.ClubGiftNotification))]
    protected void HandleClubGiftNotification(Intercept e)
    {
        if (IsActive)
            e.Block();
    }
}
