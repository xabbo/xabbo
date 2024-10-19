using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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

    private async Task<T> GetRequiredDataAsync<T>(Hotel hotel, string path, CancellationToken cancellationToken = default)
    {
        if (!path.StartsWith('/'))
            throw new ArgumentException("Path must start with '/'.", nameof(path));

        var typeInfo = JsonWebContext.Default.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>
            ?? throw new Exception($"Failed to get type info for '{typeof(T)}'.");

        var res = await _http.GetAsync($"https://{hotel.WebHost}{path}", cancellationToken);
        res.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<T>(
            res.Content.ReadAsStream(cancellationToken), typeInfo, cancellationToken)
            ?? throw new Exception($"Failed to deserialize {typeInfo.Type.Name}.");
    }

    public Task<Web.Dto.MarketplaceItemStats> FetchMarketplaceItemStats(Hotel hotel, ItemType type, string identifier, CancellationToken cancellationToken = default)
    {
        string? typeString = type switch
        {
            ItemType.Floor => "roomItem",
            ItemType.Wall => "wallItem",
            _ => throw new Exception($"Invalid item type: {type}.")
        };

        return GetRequiredDataAsync<Web.Dto.MarketplaceItemStats>(
            hotel, $"/api/public/marketplace/stats/{typeString}/{identifier}", cancellationToken);
    }

    public Task<Web.Dto.PhotoData> FetchPhotoDataAsync(Hotel hotel, string photoId, CancellationToken cancellationToken = default)
    {
        return GetRequiredDataAsync<Web.Dto.PhotoData>(
            hotel, $"/photodata/public/furni/{photoId}", cancellationToken);
    }
}