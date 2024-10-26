using ReactiveUI;

namespace Xabbo.Configuration;

public class TimingConfigBase : ReactiveObject
{
    // These timings are based on modern clients.
    [Reactive] public int FurniPlaceInterval { get; set; } = 300;
    [Reactive] public int FurniPickupInterval { get; set; } = 100;
    [Reactive] public int FurniToggleInterval { get; set; } = 150;
    [Reactive] public int FurniMoveInterval { get; set; } = 300;
    [Reactive] public int ModerationInterval { get; set; } = 500;
}