using System.Text.Json.Serialization;

namespace Xabbo.Web.Dto;

public sealed class MarketplaceItemStats
{
    [JsonPropertyName("history")]
    public List<MarketplaceItemHistoryEntry> History { get; set; } = [];

    [JsonPropertyName("statsDate")]
    public string StatsDate { get; set; } = "";

    [JsonPropertyName("soldItemCount")]
    public int SoldItemCount { get; set; }

    [JsonPropertyName("creditSum")]
    public int CreditSum { get; set; }

    [JsonPropertyName("averagePrice")]
    public int AveragePrice { get; set; }

    [JsonPropertyName("totalOpenOffers")]
    public int TotalOpenOffers { get; set; }

    [JsonPropertyName("historyLimitInDays")]
    public int HistoryLimitInDays { get; set; }
}

public sealed class MarketplaceItemHistoryEntry
{
    [JsonPropertyName("dayOffset")]
    public int DayOffset { get; set; }

    [JsonPropertyName("averagePrice")]
    public int AveragePrice { get; set; }

    [JsonPropertyName("totalSoldItems")]
    public int TotalSoldItems { get; set; }

    [JsonPropertyName("totalCreditSum")]
    public int TotalCreditSum { get; set; }

    [JsonPropertyName("totalOpenOffers")]
    public int TotalOpenOffers { get; set; }
}
