using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using ReactiveUI;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Incoming;
using Xabbo.Extension;
using Xabbo.Utility;

namespace Xabbo.ViewModels;

public sealed class RoomGiftsViewModel : ViewModelBase
{
    private readonly IExtension _ext;
    private readonly IGameDataManager _gameData;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<GiftViewModel, Id> _cache = new(x => x.Item?.Id ?? 0);

    private readonly ReadOnlyObservableCollection<GiftViewModel> _gifts;
    public ReadOnlyObservableCollection<GiftViewModel> Gifts => _gifts;

    private readonly ObservableAsPropertyHelper<bool> _isEmpty;
    public bool IsEmpty => _isEmpty.Value;

    [Reactive] public GiftViewModel? TargetGift { get; set; }

    public ReactiveCommand<Unit, Unit> PeekAllCmd { get; }
    public ReactiveCommand<GiftViewModel?, Unit> LocateGiftCmd { get; }

    public RoomGiftsViewModel(
        IExtension ext,
        IGameDataManager gameData,
        RoomManager roomManager)
    {
        _ext = ext;
        _gameData = gameData;
        _roomManager = roomManager;

        _cache
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _gifts)
            .Subscribe();

        _isEmpty = _cache.CountChanged
            .Select(count => count == 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.IsEmpty);

        _roomManager.FloorItemsLoaded += OnFloorItemsLoaded;
        _roomManager.FloorItemAdded += OnFloorItemAdded;
        _roomManager.FloorItemRemoved += OnFloorItemRemoved;
        _roomManager.Left += OnLeftRoom;

        PeekAllCmd = ReactiveCommand.Create(PeekAll);
        LocateGiftCmd = ReactiveCommand.Create<GiftViewModel?>(LocateGift);
    }

    private void ShowFurniTransition(IFloorItem item, Tile? from = null, Tile? to = null, int duration = 1000)
    {
        _ext.Send(new WiredMovementsMsg([
            new FloorItemWiredMovement
            {
                ItemId = item.Id,
                Source = from ?? item.Location,
                Destination = to ?? item.Location,
                AnimationTime = duration,
                Rotation = item.Direction,
            }
        ]));
    }

    private void LocateGift(GiftViewModel? model)
    {
        if (model is null || model.Item is null) return;

        Task.Run(async () => {
            const int delay = 75;
            for (int i = 0; i < 5; i++)
            {
                ShowFurniTransition(model.Item, to: model.Item.Location + (0, 0, 1), duration: delay);
                await Task.Delay(delay);
                ShowFurniTransition(model.Item, from: model.Item.Location + (0, 0, 1), duration: delay);
                await Task.Delay(delay);
            }
        });
    }

    private void PeekAll()
    {
        foreach (var (_, vm) in _cache.KeyValues)
        {
            vm.IsPeeking = true;
        }
    }

    private void OnLeftRoom()
    {
        _cache.Clear();
    }

    private void AddGifts(IEnumerable<IFloorItem> floorItems)
    {
        if (!Xabbo.Core.Extensions.IsInitialized)
            return;

        if (_gameData.Furni is not { } furniData ||
            _gameData.Products is not { } productData) return;

        List<GiftViewModel> gifts = [];

        foreach (var item in floorItems.OfCategory(FurniCategory.Gift))
        {
            GiftViewModel gift = new GiftViewModel(item);

            if (gift.ProductCode is not null)
            {
                gift.CanPeek = true;

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