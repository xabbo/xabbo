using ReactiveUI;

namespace Xabbo.Configuration;

public class TimingConfigBase : ReactiveObject
{
    [Reactive] public int FurniPlaceInterval { get; set; } = 150;
    [Reactive] public int FurniPickupInterval { get; set; } = 80;
    [Reactive] public int FurniToggleInterval { get; set; } = 150;
    [Reactive] public int FurniRotateInterval { get; set; } = 150;
    [Reactive] public int ModerationInterval { get; set; } = 500;
}