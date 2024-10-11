namespace Xabbo.Configuration;

public sealed class ModernTimingConfig : TimingConfigBase
{
    [Reactive] public int BounceUnbanDelay { get; set; } = 500;

    public ModernTimingConfig()
    {
        ModerationInterval = 500;
        FurniPickupInterval = 80;
    }
}