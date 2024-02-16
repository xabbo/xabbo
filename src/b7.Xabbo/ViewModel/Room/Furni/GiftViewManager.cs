using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Events;
using Xabbo.Core.Extensions;

using b7.Xabbo.Services;
using System.Linq;
using Xabbo.Messages;

namespace b7.Xabbo.ViewModel;

public class GiftViewManager : ComponentViewModel
{
    private readonly IUiContext _uiContext;
    private readonly IGameDataManager _gameData;
    private readonly RoomManager _roomManager;

    private readonly Dictionary<long, GiftViewModel> _idMap = new();

    public ObservableCollection<GiftViewModel> Gifts { get; } = new();

    public GiftViewManager(IExtension extension, IUiContext uiContext, IGameDataManager gameData, RoomManager roomManager)
        : base(extension)
    {
        _uiContext = uiContext;
        _gameData = gameData;
        _roomManager = roomManager;

        _roomManager.FloorItemsLoaded += OnFloorItemsLoaded;
        _roomManager.FloorItemAdded += OnFloorItemAdded;
        _roomManager.FloorItemRemoved += OnFloorItemRemoved;
        _roomManager.Left += OnLeftRoom;
    }

    private void OnLeftRoom(object? sender, EventArgs e)
    {
        _uiContext.Invoke(() =>
        {
            _idMap.Clear();
            Gifts.Clear();
        });
    }

    private void AddGifts(IEnumerable<IFloorItem> floorItems)
    {
        if (_gameData.Furni is null || _gameData.Products is null) return;

        List<GiftViewModel> gifts = new();

        foreach (var item in floorItems.OfCategory(FurniCategory.Gift))
        {
            if (item.Data is not MapData map) continue;

            GiftViewModel gift = new GiftViewModel() { Item = item };

            if (map.TryGetValue("PURCHASER_NAME", out string? senderName))
                gift.SenderName = senderName;

            if (map.TryGetValue("MESSAGE", out string? message))
                gift.Message = message;

            if (map.TryGetValue("PURCHASER_FIGURE", out string? figureString))
                gift.SenderImageUri = new Uri($"https://www.habbo.com/habbo-imaging/avatarimage?figure={figureString}&headonly=1&dir=2");

            if (map.TryGetValue("PRODUCT_CODE", out string? productCode))
            {
                if (_gameData.Furni.TryGetInfo(productCode, out FurniInfo? furniInfo))
                {

                    gift.ItemName = furniInfo.Name;
                }
                else if (_gameData.Products.TryGetValue(productCode, out ProductInfo? product))
                {
                    gift.ItemName = product.Name;
                    var furniInfos = _gameData.Furni.Where(x => x.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (furniInfos.Length == 1)
                        furniInfo = furniInfos[0];
                }

                if (furniInfo is not null)
                {
                    string identifier = furniInfo.Identifier.Replace('*', '_');
                    gift.ItemIdentifier = furniInfo.Identifier;
                    gift.ItemImageUri = new Uri($"https://images.habbo.com/dcr/hof_furni/{furniInfo.Revision}/{identifier}_icon.png");
                }
            }

            gifts.Add(gift);
        }

        if (gifts.Count > 0)
        {
            _uiContext.Invoke(() =>
            {
                foreach (var gift in gifts)
                {
                    if (_idMap.TryAdd(gift.Item.Id, gift))
                    {
                        Gifts.Add(gift);
                    }
                }
            });
        }
    }

    private void OnFloorItemsLoaded(object? sender, FloorItemsEventArgs e) => AddGifts(e.Items);

    private void OnFloorItemAdded(object? sender, FloorItemEventArgs e) => AddGifts(new[] { e.Item });

    private void OnFloorItemRemoved(object? sender, FloorItemEventArgs e)
    {
        _uiContext.Invoke(() =>
        {
            if (_idMap.Remove(e.Item.Id, out GiftViewModel? giftViewModel))
            {
                Gifts.Remove(giftViewModel);
            }
        });
    }
}
