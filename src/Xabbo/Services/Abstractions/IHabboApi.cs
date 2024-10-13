using Xabbo.Core;

using MarketplaceItemStats = Xabbo.Web.Dto.MarketplaceItemStats;

namespace Xabbo.Services.Abstractions;

public interface IHabboApi
{
    Task<MarketplaceItemStats> FetchMarketplaceItemStats(Hotel hotel, ItemType type, string identifier);
}