using Xabbo.Core;
using Xabbo.Web.Dto;

using MarketplaceItemStats = Xabbo.Web.Dto.MarketplaceItemStats;

namespace Xabbo.Services.Abstractions;

public interface IHabboApi
{
    Task<MarketplaceItemStats> FetchMarketplaceItemStats(Hotel hotel, ItemType type, string identifier, CancellationToken cancellationToken = default);
    Task<PhotoData> FetchPhotoData(Hotel hotel, string photoId, CancellationToken cancellationToken = default);
}