using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class FurniConfig : ReactiveObject
{
    [Reactive] public int PickupInterval { get; set; } = 100;
    [Reactive] public int PickupIntervalOrigins { get; set; } = 750;
}
