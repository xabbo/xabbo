using Xabbo.Messages.Flash;
using Xabbo.Extension;

namespace Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class AntiHcGiftNotificationComponent(IExtension extension) : Component(extension)
{
    [Reactive] public bool Enabled { get; set; } = true;

    [InterceptIn(nameof(In.ClubGiftNotification))]
    protected void HandleClubGiftNotification(Intercept e)
    {
        if (Enabled)
            e.Block();
    }
}
