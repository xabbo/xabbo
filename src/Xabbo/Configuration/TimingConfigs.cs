using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class TimingConfigs : ReactiveObject
{
    [Reactive] public ModernTimingConfig Modern { get; set; } = new();
    [Reactive] public OriginsTimingConfig Origins { get; set; } = new();
}