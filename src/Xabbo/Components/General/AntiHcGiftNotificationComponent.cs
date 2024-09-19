using Microsoft.Extensions.Configuration;

using Xabbo.Messages.Flash;
using Xabbo.Extension;

namespace Xabbo.Components;

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
