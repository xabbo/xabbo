using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class MovementConfig : ReactiveObject
{
    [Reactive] public bool NoTurn { get; set; } = true;
    [Reactive] public bool TurnOnReselectUser { get; set; } = true;
    [Reactive] public double ReselectThreshold { get; set; } = 1.0;
    [Reactive] public bool NoWalk { get; set; } = false;
    [Reactive] public bool TurnTowardsClickedTile { get; set; } = false;
}
