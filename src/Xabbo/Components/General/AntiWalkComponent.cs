using Xabbo.Extension;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Services.Abstractions;
using Xabbo.Configuration;

namespace Xabbo.Components;

[Intercept]
public partial class AntiWalkComponent(
    IExtension extension,
    IConfigProvider<AppConfig> config
)
    : Component(extension)
{
    [Intercept]
    private void OnMove(Intercept<WalkMsg> e)
    {
        if (config.Value.Movement.NoWalk)
        {
            e.Block();

            if (config.Value.Movement.TurnTowardsClickedTile)
                Ext.Send(new LookToMsg(e.Msg.Point));
        }
    }
}
