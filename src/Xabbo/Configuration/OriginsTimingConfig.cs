namespace Xabbo.Configuration;

public sealed class OriginsTimingConfig : TimingConfigBase
{
    public OriginsTimingConfig()
    {
        ModerationInterval = 750;
        FurniPickupInterval = 560;
    }
}