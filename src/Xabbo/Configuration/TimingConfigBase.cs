using ReactiveUI;

namespace Xabbo.Configuration;

public class TimingConfigBase : ReactiveObject
{
    [Reactive] public int FurniPickupInterval { get; set; } = 80;
    [Reactive] public int ModerationInterval { get; set; } = 500;
}