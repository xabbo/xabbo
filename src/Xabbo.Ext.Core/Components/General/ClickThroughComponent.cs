using Microsoft.Extensions.Configuration;

using ReactiveUI;

using Xabbo.Extension;
using Xabbo.Messages.Flash;

namespace Xabbo.Ext.Components;

[Intercept(~ClientType.Shockwave)]
public partial class ClickThroughComponent : Component
{
    public ClickThroughComponent(IExtension extension,
        IConfiguration config)
        : base(extension)
    {
        IsActive = config.GetValue("ClickThrough:Active", false);

        this.ObservableForProperty(x => x.IsActive)
            .Subscribe(x => OnIsActiveChanged(x.Value));
    }

    protected override void OnConnected(GameConnectedArgs e)
    {
        base.OnConnected(e);

        IsAvailable = Client is not ClientType.Shockwave;
    }

    // [RequiredIn(nameof(In.GameYouArePlayer))]
    protected void OnIsActiveChanged(bool isActive)
    {
        Ext.Send(In.YouArePlayingGame, isActive);
    }

    [InterceptIn(nameof(In.RoomEntryInfo))]
    // [RequiredIn(nameof(In.GameYouArePlayer))]
    private void OnEnterRoom(Intercept e)
    {
        if (IsActive)
            Ext.Send(In.YouArePlayingGame, true);
    }
}
