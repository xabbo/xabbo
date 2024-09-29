using Microsoft.Extensions.Configuration;
using ReactiveUI;

using Xabbo.Messages.Flash;
using Xabbo.Extension;

namespace Xabbo.Components;

[Intercept(~ClientType.Shockwave)]
public partial class ClickThroughComponent : Component
{
    [Reactive] public bool Enabled { get; set; }

    public ClickThroughComponent(IExtension extension)
        : base(extension)
    {
        this.ObservableForProperty(x => x.Enabled)
            .Subscribe(x => OnIsActiveChanged(x.Value));
    }

    protected override void OnConnected(ConnectedEventArgs e)
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
        if (Enabled)
            Ext.Send(In.YouArePlayingGame, true);
    }
}
