using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Avalonia.Controls.Selection;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using Humanizer;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Configuration;
using Xabbo.Controllers;
using Xabbo.Services.Abstractions;
using Xabbo.Web.Serialization;
using Xabbo.Utility;
using Xabbo.Exceptions;

namespace Xabbo.ViewModels;

[Intercept]
public sealed partial class InventoryViewModel : ControllerBase
{
    public enum State { None, Loading, AwaitingCornerSelection, AutoPlacing, ManualPlacing, Offering }

    private readonly ILogger _logger;
    private readonly IConfigProvider<AppConfig> _config;
    private readonly IDialogService _dialogService;
    private readonly IHabboApi _api;
    private readonly IOperationManager _operations;
    private readonly FurniPlacementController _placement;
    private readonly IPlacementFactory _placementFactory;
    private readonly RoomManager _roomManager;
    private readonly InventoryManager _inventoryManager;
    private readonly TradeManager _tradeManager;

    private readonly SourceCache<InventoryStackViewModel, ItemDescriptor> _cache = new(x => x.Item.GetDescriptor());

    private readonly ReadOnlyObservableCollection<InventoryStackViewModel> _stacks;
    public ReadOnlyObservableCollection<InventoryStackViewModel> Stacks => _stacks;
    [Reactive] public int ItemCount { get; set; }

    private readonly SourceCache<PhotoViewModel, Id> _photoCache = new(x => x.Item.Id);
    private readonly ReadOnlyObservableCollection<PhotoViewModel> _photos;
    public ReadOnlyObservableCollection<PhotoViewModel> Photos => _photos;

    [Reactive] public string FilterText { get; set; } = "";

    public SelectionModel<InventoryStackViewModel> Selection { get; } = new() { SingleSelect = false };
    public SelectionModel<PhotoViewModel> PhotoSelection { get; } = new() { SingleSelect = false };

    private readonly ObservableAsPropertyHelper<string> _itemCountText;
    public string ItemCountText => _itemCountText.Value;

    private readonly ObservableAsPropertyHelper<bool> _isBusy;
    public bool IsBusy => _isBusy.Value;

    private readonly ObservableAsPropertyHelper<string?> _emptyText;
    public string? EmptyText => _emptyText.Value;

    private readonly ObservableAsPropertyHelper<string?> _emptyPhotoStatus;
    public string? EmptyPhotoStatus => _emptyPhotoStatus.Value;

    private readonly ObservableAsPropertyHelper<string> _statusText;
    public string StatusText => _statusText.Value;

    [Reactive] public State Status { get; set; } = State.None;
    [Reactive] public int Progress { get; set; }
    [Reactive] public int MaxProgress { get; set; }

    [Reactive] public bool HasLoaded { get; set; }

    public ReactiveCommand<Unit, Task> LoadCmd { get; }
    public ReactiveCommand<Unit, Task> OfferItemsCmd { get; }
    public ReactiveCommand<Unit, Task> OfferPhotosCmd { get; }
    public ReactiveCommand<string, Task> PlaceItemsCmd { get; }
    public ReactiveCommand<Unit, Unit> CancelCmd { get; }

    public InventoryViewModel(
        IExtension extension,
        ILoggerFactory loggerFactory,
        IConfigProvider<AppConfig> config,
        IDialogService dialogService,
        IHabboApi api,
        IOperationManager operations,
        FurniPlacementController placement,
        IPlacementFactory placementFactory,
        RoomManager roomManager,
        InventoryManager inventoryManager,
        TradeManager tradeManager
    )
        : base(extension)
    {
        _logger = loggerFactory.CreateLogger<InventoryViewModel>();
        _config = config;
        _dialogService = dialogService;
        _api = api;
        _operations = operations;
        _placement = placement;
        _placementFactory = placementFactory;
        _roomManager = roomManager;
        _inventoryManager = inventoryManager;
        _tradeManager = tradeManager;
        _tradeManager.Updated += OnTradeUpdated;
        _tradeManager.Closed += OnTradeClosed;

        var comparer = SortExpressionComparer<InventoryStackViewModel>.Ascending(x => x.Name);

        _cache
            .Connect()
            .Filter(this
                .WhenAnyValue(x => x.FilterText)
                .Select(CreateFilter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _stacks, comparer)
            .Subscribe();

        _photoCache
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _photos)
            .Subscribe();

        _isBusy = this
            .WhenAnyValue(x => x.Status, status => status is not State.None)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.IsBusy);

        _emptyText =
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.HasLoaded),
                _cache.CountChanged,
                _roomManager.WhenAnyValue(x => x.IsInRoom),
                (hasLoaded, count, isInRoom) => {
                    if (count > 0)
                        return null;
                    if (hasLoaded)
                        return "No items";
                    if (!isInRoom)
                        return "Enter a room to load inventory";
                    return null;
                }
            )
            .ToProperty(this, x => x.EmptyText);

        _emptyPhotoStatus =
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.HasLoaded),
                _photoCache.CountChanged,
                (hasLoaded, count) =>
                    (hasLoaded && count == 0) ? "No photos in inventory" : ""
            )
            .ToProperty(this, x => x.EmptyPhotoStatus);


        _statusText =
            Observable.CombineLatest(
                this.WhenAnyValue(
                    x => x.Status,
                    x => x.Progress,
                    x => x.MaxProgress,
                    (status, progress, maxProgress) => (status, progress, maxProgress)
                ),
                _inventoryManager.WhenAnyValue(
                    x => x.CurrentProgress,
                    x => x.MaxProgress,
                    (current, max) => (current, max)
                ),
                _placement.WhenAnyValue(
                    x => x.Progress,
                    x => x.MaxProgress,
                    x => x.Status,
                    (progress, maxProgress, status) => (progress, maxProgress, status)
                ),
                (self, manager, placer) => self.status switch
                {
                    State.Loading => manager.max > 0
                        ? $"Loading...\n{manager.current} / {manager.max}"
                        : $"Loading...\n{manager.current} / ?",
                    State.Offering => $"Offering items...\n{self.progress} / {self.maxProgress}",
                    State.AwaitingCornerSelection =>
                        $"Click the 2 corner tiles of the area where you want to place the items"
                        + $"\n{self.progress} / {self.maxProgress}",
                    State.ManualPlacing => $"{(
                        placer.status is FurniPlacementController.State.PlacingFloorItems
                        ? "Placing floor items"
                        : "Placing wall items"
                    )}\nClick tiles to place items...\n{placer.progress} / {placer.maxProgress}",
                    State.AutoPlacing => $"Placing items...\n{placer.progress} / {placer.maxProgress}",
                    _ => ""
                }
            )
            .ToProperty(this, x => x.StatusText);

        _itemCountText =
            Observable.CombineLatest(
                _cache.CountChanged,
                this.WhenAnyValue(x => x.ItemCount),
                (stackCount, itemCount) => $"{stackCount} furni, {"item".ToQuantity(itemCount)}"
            )
            .ToProperty(this, x => x.ItemCountText);

        LoadCmd = ReactiveCommand.Create<Task>(
            LoadInventoryAsync,
            Observable.CombineLatest(
                _roomManager.WhenAnyValue(x => x.IsInRoom),
                this.WhenAnyValue(x => x.IsBusy),
                (isInRoom, isBusy) => isInRoom && !isBusy
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        OfferItemsCmd = ReactiveCommand.Create<Task>(
            OfferItemsAsync,
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.IsBusy),
                _tradeManager.WhenAnyValue(x => x.IsTrading),
                (isBusy, isTrading) => !isBusy && isTrading
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        OfferPhotosCmd = ReactiveCommand.Create<Task>(
            OfferPhotosAsync,
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.IsBusy),
                _tradeManager.WhenAnyValue(x => x.IsTrading),
                PhotoSelection.WhenValueChanged(x => x.SelectedItems),
                (isBusy, isTrading, selection) => !isBusy && isTrading && selection?.Count > 0
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        PlaceItemsCmd = ReactiveCommand.Create<string, Task>(
            PlaceItemsAsync,
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.IsBusy),
                _roomManager.WhenAnyValue(x => x.IsInRoom),
                _tradeManager.WhenAnyValue(x => x.IsTrading),
                (isBusy, isInRoom, isTrading) => !isBusy && isInRoom && !isTrading
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        CancelCmd = ReactiveCommand.Create(OnCancel);

        _inventoryManager.Loaded += OnInventoryLoaded;
        _inventoryManager.Cleared += OnInventoryCleared;
        _inventoryManager.ItemAdded += OnItemAdded;
        _inventoryManager.ItemRemoved += OnItemRemoved;
    }

    private void OnCancel()
    {
        _operations.TryCancelOperation(out _);
    }

    private Func<InventoryStackViewModel, bool> CreateFilter(string? filterText)
    {
        return (vm) => {
            if (string.IsNullOrWhiteSpace(filterText))
                return true;
            return vm.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        };
    }

    private void AddPhotos(IEnumerable<IInventoryItem> items)
    {
        _photoCache.Edit(cache => {
            cache.AddOrUpdate(
                items
                    .OfType<IInventoryItem>()
                    .OfKind("external_image_wallitem_poster_small")
                    .Select(TryExtractPhotoId)
                    .Where(it => !string.IsNullOrWhiteSpace(it.PhotoId))
                    .Select(it => new PhotoViewModel(
                        it.Item,
                        new(() => FetchPhotoUrlAsync(Session.Hotel, it.PhotoId!))
                    ))
            );
        });
    }

    private void OnInventoryLoaded()
    {
        if (_inventoryManager.Inventory is not { } inventory)
            return;

        _cache.Edit(cache => {
            cache.Clear();
            cache.AddOrUpdate(
                inventory
                    .GroupBy(item => item.GetDescriptor())
                    .Select(group => {
                        int count;
                        if (group.Key.TryGetInfo(out FurniInfo? info) && info.Category == FurniCategory.Sticky)
                        {
                            count = group.Sum(x => int.TryParse(x.Data.Value, out int n) ? n : 0);
                        }
                        else
                        {
                            count = group.Count();
                        }
                        return new InventoryStackViewModel(group.Key, group);
                    })
            );
        });

        _photoCache.Clear();
        AddPhotos(inventory);

        ItemCount = inventory.Count;
        HasLoaded = true;
    }

    private void OnTradeUpdated(TradeUpdatedEventArgs e)
    {
        foreach (var group in e.SelfOffer.GroupBy(x => x.GetDescriptor()))
        {
            _cache
                .Lookup(group.Key)
                .IfHasValue(vm => {
                    vm.OfferCount = group.Count();
                })
                .Else(() => {
                    _logger.LogWarning("Failed to find item {Descriptor} to update offer count.", group.Key);
                });
        }
    }

    private void OnTradeClosed(TradeClosedEventArgs args)
    {
        _cache.Edit(cache => {
            foreach (var (k, vm) in cache.KeyValues)
                vm.OfferCount = 0;
        });
    }

    private static (IInventoryItem Item, string? PhotoId) TryExtractPhotoId(IInventoryItem item)
    {
        string? photoId = null;

        try { photoId = JsonSerializer.Deserialize(item.Data.Value, JsonWebContext.Default.PhotoInfo)?.Id; }
        catch { }

        return (item, photoId);
    }

    private async Task<string?> FetchPhotoUrlAsync(Hotel hotel, string photoId)
        => (await _api.FetchPhotoDataAsync(hotel, photoId)).Url;

    private void OnInventoryCleared()
    {
        _cache.Clear();
        _photoCache.Clear();
        HasLoaded = false;
        ItemCount = 0;
    }

    private async Task OfferItemsAsync()
    {
        var viewModel = _dialogService.CreateViewModel<OfferItemsViewModel>();
        viewModel.Items = Selection.SelectedItems
            .Where(stack => stack is not null)
            .Select(stack => new OfferItemViewModel(stack!.Item) {
                Amount = stack.OfferCount > 0 ? stack.OfferCount : stack.Count,
                MinAmount = stack.OfferCount,
                MaxAmount = stack.Count,
            }).ToList();

        var result = await _dialogService.ShowContentDialogAsync(_dialogService.CreateViewModel<MainViewModel>(),
            new HanumanInstitute.MvvmDialogs.Avalonia.Fluent.ContentDialogSettings
            {
                Title = "Offer items",
                Content = viewModel,
                PrimaryButtonText = "Cancel",
                SecondaryButtonText = "Offer",
                FullSizeDesired = true
            }
        );

        if (result == FluentAvalonia.UI.Controls.ContentDialogResult.Secondary)
        {
            await OfferItemsAsync(viewModel.Items);
        }
    }

    private async Task OfferPhotosAsync()
    {
        await OfferItemsAsync(
            PhotoSelection.SelectedItems
            .Select(x => x?.Item)
            .OfType<IInventoryItem>()
        );
    }

    private Task PlaceItemsAsync(string mode) => PlaceItemsAsync(mode, Selection.SelectedItems);

    private async Task<Area> SelectAreaAsync(CancellationToken cancellationToken)
    {
        Progress = 0;
        MaxProgress = 2;
        Status = State.AwaitingCornerSelection;

        return await _operations.RunAsync("Select area", async (ct) => {
            Point[] corners = new Point[2];
            for (int i = 0; i < 2; i++)
            {
                Progress = i+1;
                corners[i] = (await ReceiveAsync<WalkMsg>(timeout: -1, block: true, cancellationToken: ct)).Point;
            }
            return new Area(corners[0], corners[1]);
        }, cancellationToken);
    }

    private async Task PlaceItemsAsync(string mode, IReadOnlyList<InventoryStackViewModel?> selectedItems)
    {
        try
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            var items = selectedItems
                .Where(x => x is not null)
                .SelectMany(x => x!)
                .ToArray();

            if (items.Length == 0)
                return;

            if (!_roomManager.EnsureInRoom(out var room))
                throw new Exception("Room state is not being tracked.");

            var errorHandling = FurniPlacementController.ErrorHandling.Abort;

            IFloorItemPlacement floorPlacement;
            IWallItemPlacement wallPlacement;

            switch (mode)
            {
                case "anywhere":
                    wallPlacement = _placementFactory.CreateRandomWallPlacement(room.FloorPlan.Area);
                    floorPlacement = _placementFactory.CreateAreaFloorPlacement(room.FloorPlan.Area);
                    Status = State.AutoPlacing;
                    break;
                case "area":
                    Area placementArea = await SelectAreaAsync(cancellationToken);
                    wallPlacement = _placementFactory.CreateRandomWallPlacement(placementArea);
                    floorPlacement = _placementFactory.CreateAreaFloorPlacement(placementArea);
                    Status = State.AutoPlacing;
                    break;
                case "manual":
                    floorPlacement = _placementFactory.CreateManualFloorPlacement();
                    wallPlacement = _placementFactory.CreateManualWallPlacement();
                    errorHandling = FurniPlacementController.ErrorHandling.Retry;
                    Status = State.ManualPlacing;
                    break;
                default:
                    throw new Exception($"Unknown placement mode: '{mode}'.");
            }

            using (floorPlacement)
            using (wallPlacement)
            {
                await _operations.RunAsync("Place furni",
                    (ct) => _placement.PlaceItemsAsync(
                        items, floorPlacement, wallPlacement, errorHandling, ct),
                    cancellationToken
                );
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Status = State.None;
            if (ex is TimeoutException)
                await _dialogService.ShowAsync("Timed out", "Try increasing the furni placement interval in settings.");
            else if (ex is PlacementNotFoundException)
                await _dialogService.ShowAsync("No placement location", "Could not find a valid placement location.");
            else
                await _dialogService.ShowAsync("Error", ex.Message);
        }
        finally
        {
            Status = State.None;
        }
    }

    private async Task OfferItemsAsync(IEnumerable<OfferItemViewModel> offers)
    {
        if (!_tradeManager.IsTrading)
        {
            await _dialogService.ShowAsync("Warning", "You are not currently in a trade.");
            return;
        }

        HashSet<Id> offered = [];
        if (_tradeManager.SelfOffer is { } selfOffer)
        {
            foreach (var item in selfOffer)
                offered.Add(item.ItemId);
        }

        List<IInventoryItem> toOffer = [];
        foreach (var offer in offers)
        {
            _cache
                .Lookup((ItemDescriptor)offer.Item)
                .IfHasValue(stack => {
                    int addAmount = offer.Amount - offer.MinAmount;
                    toOffer.Add(stack
                        .Where(x => offered.Add(x.ItemId))
                        .Take(addAmount));
                });
        }

        await OfferItemsAsync(toOffer);
    }

    private async Task OfferItemsAsync(IEnumerable<IInventoryItem> items)
    {
        var array = items.ToArray();

        try
        {
            await _operations.RunAsync("Offer items", async (ct) => {
                try
                {
                    Progress = 0;
                    MaxProgress = array.Length;
                    Status = State.Offering;

                    if (Session.Is(ClientType.Origins))
                        await OfferItemsOriginsAsync(array);
                    else
                        await OfferItemsModernAsync(array);
                }
                finally
                {
                    Status = State.None;
                    Progress = 0;
                    MaxProgress = 0;
                }
            });
        }
        catch (TimeoutException)
        {
            await _dialogService.ShowAsync("Timed out", "Try increasing the offer interval in settings.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAsync("Error", ex.Message);
        }
    }

    private async Task OfferItemsOriginsAsync(IInventoryItem[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            Progress = i;
            if (i > 0)
                await Task.Delay(_config.Value.Timing.Origins.TradeOfferInterval);
            Send(new OfferTradeItemMsg(items[i]));
        }
    }

    private Task OfferItemsModernAsync(IInventoryItem[] items)
    {
        Send(new OfferTradeItemsMsg(items));
        return Task.CompletedTask;
    }

    private void OnItemAdded(InventoryItemEventArgs e)
    {
        _cache.Edit(cache => {
            var descriptor = e.Item.GetDescriptor();
            _logger.LogDebug("Looking up descriptor {Descriptor}.", descriptor);
            cache.Lookup(descriptor)
                .IfHasValue(vm => {
                    _logger.LogDebug("Adding item to existing stack.");
                    if (!vm.Add(e.Item))
                        _logger.LogWarning("Failed to add item #{ItemId} to existing stack.", e.Item.ItemId);
                })
                .Else(() => {
                    _logger.LogDebug("Creating new stack with item #{ItemId}.", e.Item.ItemId);
                    cache.AddOrUpdate(new InventoryStackViewModel(descriptor, [e.Item]));
                });
        });

        AddPhotos([e.Item]);

        ItemCount = _inventoryManager.Inventory?.Count ?? 0;
    }

    private void OnItemRemoved(InventoryItemEventArgs e)
    {
        var descriptor = e.Item.GetDescriptor();
        _cache.Edit(cache => {
            cache.Lookup(descriptor)
                .IfHasValue(vm => {
                    if (vm.Remove(e.Item))
                    {
                        _logger.LogDebug("Removed item #{ItemId} from stack {Descriptor}.",
                            e.Item.ItemId, descriptor);
                        if (vm.Count == 0)
                        {
                            _logger.LogDebug("Removed empty stack {Descriptor}.", descriptor);
                            cache.Remove(vm);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to remove item #{ItemId} from stack {Descriptor}.",
                            e.Item.ItemId, descriptor);
                    }
                })
                .Else(() => {
                    _logger.LogWarning("Failed to get stack for {Descriptor}.", descriptor);
                });
        });

        _photoCache.RemoveKey(e.Item.ItemId);

        ItemCount = _inventoryManager.Inventory?.Count ?? 0;
    }

    public async Task LoadInventoryAsync()
    {
        if (!_roomManager.IsInRoom)
        {
            await _dialogService.ShowAsync("Warning", "You must be in a room to load your inventory.");
            return;
        }

        try
        {
            Status = State.Loading;
            await _operations.RunAsync("Load inventory",
                (ct) => _inventoryManager.LoadInventoryAsync(timeout: 120000, forceReload: true, cancellationToken: ct));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await _dialogService.ShowAsync("Failed to load inventory", ex.Message);
        }
        finally
        {
            Status = State.None;
        }
    }
}