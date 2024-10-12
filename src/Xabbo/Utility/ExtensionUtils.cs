using Xabbo.Core;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Extension;

namespace Xabbo.Utility;

public static class ExtensionUtils
{
    public static void SlideFurni(
        this IExtension ext,
        IFloorItem item, Tile? from = null, Tile? to = null, int duration = 1000)
    {
        if (ext.Session.Is(ClientType.Modern))
        {
            ext.Send(new WiredMovementsMsg {
                new FloorItemWiredMovement {
                    ItemId = item.Id,
                    Source = from ?? item.Location,
                    Destination = to ?? item.Location,
                    AnimationTime = duration,
                    Rotation = item.Direction
                }
            });
        }
    }
}