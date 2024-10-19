namespace Xabbo.Configuration;

public sealed class OriginsTimingConfig : TimingConfigBase
{
    [Reactive] public int TradeOfferInterval { get; set; } = 600;

    public OriginsTimingConfig()
    {
        ModerationInterval = 750;
        FurniPickupInterval = 560;
    }
}