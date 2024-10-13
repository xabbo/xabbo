using System.Text.Json;

using Xabbo.Core;
using Xabbo.Services.Abstractions;
using Xabbo.Web.Serialization;

namespace Xabbo.Services;

public sealed class HabboApi : IHabboApi
{
    private readonly HttpClient _http = new()
    {
        DefaultRequestHeaders = {
            { "User-Agent", "xabbo" }
        }
    };

    public async Task<Web.Dto.MarketplaceItemStats> FetchMarketplaceItemStats(Hotel hotel, ItemType type, string identifier)
    {
        string? typeString = type switch
        {
            ItemType.Floor => "roomItem",
            ItemType.Wall => "wallItem",
            _ => throw new Exception($"Invalid item type: {type}.")
        };

        var res = await _http.GetAsync($"https://{hotel.WebHost}/api/public/marketplace/stats/{typeString}/{identifier}");
        res.EnsureSuccessStatusCode();

        var stats = await JsonSerializer.DeserializeAsync(
            await res.Content.ReadAsStreamAsync(), JsonWebContext.Default.MarketplaceItemStats)
            ?? throw new Exception($"Failed to deserialize {nameof(MarketplaceItemStats)}");

        return stats;
    }
}