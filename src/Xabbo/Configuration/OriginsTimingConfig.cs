namespace Xabbo.Configuration;

public sealed class OriginsTimingConfig : TimingConfigBase
{
    [Reactive] public int TradeOfferInterval { get; set; } = 600;

    public OriginsTimingConfig()
    {
        ModerationInterval = 750;
        FurniPlaceInterval = 550;
        FurniPickupInterval = 550;
        FurniToggleInterval = 500;
        FurniMoveInterval = 500;
    }
}