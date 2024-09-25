using System.Collections.ObjectModel;
using DynamicData;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Utility;

namespace Xabbo.ViewModels;

public sealed class RoomGiftsViewModel : ViewModelBase
{
    private readonly IGameDataManager _gameData;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<GiftViewModel, Id> _cache = new(x => x.Item?.Id ?? 0);

    private readonly ReadOnlyObservableCollection<GiftViewModel> _gifts;
    public ReadOnlyObservableCollection<GiftViewModel> Gifts => _gifts;

    public RoomGiftsViewModel(
        IGameDataManager gameData,
        RoomManager roomManager)
    {
        _gameData = gameData;
        _roomManager = roomManager;

        _cache
            .Connect()
            .Bind(out _gifts)
            .Subscribe();

        _roomManager.FloorItemsLoaded += OnFloorItemsLoaded;
        _roomManager.FloorItemAdded += OnFloorItemAdded;
        _roomManager.FloorItemRemoved += OnFloorItemRemoved;
        _roomManager.Left += OnLeftRoom;
    }

    private void OnLeftRoom()
    {
        _cache.Clear();
    }

    private void AddGifts(IEnumerable<IFloorItem> floorItems)
    {
        if (_gameData.Furni is not { } furniData ||
            _gameData.Products is not { } productData) return;

        List<GiftViewModel> gifts = [];

        foreach (var item in floorItems.OfCategory(FurniCategory.Gift))
        {
            if (item.Data is not MapData map) continue;

            GiftViewModel gift = new GiftViewModel(item);

            if (gift.ProductCode is not null)
            {
                if (furniData.TryGetInfo(gift.ProductCode, out FurniInfo? furniInfo))
                {
                    gift.ItemName = furniInfo.Name;
                }
                else if (productData.TryGetValue(gift.ProductCode, out ProductInfo? product))
                {
                    gift.ItemName = product.Name;
                    var furniInfos = furniData
                        .Where(x => x.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    if (furniInfos.Length == 1)
                        furniInfo = furniInfos[0];
                }

                if (furniInfo is not null)
                {
                    gift.ItemIdentifier = furniInfo.Identifier;
                    gift.ItemImageUrl = UrlHelper.FurniIconUrl(furniInfo.Identifier, furniInfo.Revision);
                }
            }

            gifts.Add(gift);
        }

        _cache.AddOrUpdate(gifts);
    }

    private void OnFloorItemsLoaded(FloorItemsEventArgs e) => AddGifts(e.Items);
    private void OnFloorItemAdded(FloorItemEventArgs e) => AddGifts([e.Item]);
    private void OnFloorItemRemoved(FloorItemEventArgs e) => _cache.RemoveKey(e.Item.Id);
}