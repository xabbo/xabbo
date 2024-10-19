using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Avalonia.Controls.Selection;
using DynamicData;
using DynamicData.Kernel;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

using Xabbo.Extension;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Configuration;
using Xabbo.Controllers;
using Xabbo.Services.Abstractions;
using System.Text.Json;
using Xabbo.Web.Serialization;

namespace Xabbo.ViewModels;

[Intercept]
public sealed partial class InventoryViewModel : ControllerBase
{
    public enum State { None, Loading, Placing, Offering }

    private readonly ILogger _logger;
    private readonly IConfigProvider<AppConfig> _config;
    private readonly IDialogService _dialogService;
    private readonly IHabboApi _api;
    private readonly InventoryManager _inventoryManager;

    private readonly SourceCache<InventoryStackViewModel, ItemDescriptor> _cache = new(x => x.Item.GetDescriptor());

    private readonly ReadOnlyObservableCollection<InventoryStackViewModel> _stacks;
    public ReadOnlyObservableCollection<InventoryStackViewModel> Stacks => _stacks;
    [Reactive] public int ItemCount { get; set; }

    private readonly SourceCache<InventoryPhotoViewModel, Id> _photoCache = new(x => x.Item.Id);
    private readonly ReadOnlyObservableCollection<InventoryPhotoViewModel> _photos;
    public ReadOnlyObservableCollection<InventoryPhotoViewModel> Photos => _photos;

    [Reactive] public string FilterText { get; set; } = "";

    public ReactiveCommand<Unit, Task> LoadCmd { get; }
    public ReactiveCommand<Unit, Task> OfferItemsCmd { get; }
    public ReactiveCommand<Unit, Task> PlaceItemsCmd { get; }

    public InventoryManager Manager => _inventoryManager;

    public SelectionModel<InventoryStackViewModel> Selection { get; } = new SelectionModel<InventoryStackViewModel>() { SingleSelect = false };

    private readonly ObservableAsPropertyHelper<bool> _isBusy;
    public bool IsBusy => _isBusy.Value;

    [Reactive] public State Status { get; set; } = State.None;
    [Reactive] public int Progress { get; set; }
    [Reactive] public int MaxProgress { get; set; }

    private readonly ObservableAsPropertyHelper<string> _statusText;
    public string StatusText => _statusText.Value;

    public InventoryViewModel(
        IExtension extension,
        ILoggerFactory loggerFactory,
        IConfigProvider<AppConfig> config,
        IDialogService dialogService,
        IHabboApi api,
        InventoryManager inventoryManager
    )
        : base(extension)
    {
        _logger = loggerFactory.CreateLogger<InventoryViewModel>();
        _config = config;
        _dialogService = dialogService;
        _api = api;
        _inventoryManager = inventoryManager;

        _cache
            .Connect()
            .Filter(this
                .WhenAnyValue(x => x.FilterText)
                .Select(CreateFilter))
            .SortBy(x => x.Name)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _stacks)
            .Subscribe();

        _photoCache
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _photos)
            .Subscribe();

        this.WhenAnyValue(x => x.FilterText)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => _cache.Refresh());

        _isBusy = this
            .WhenAnyValue(x => x.Status, status => status is not State.None)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.IsBusy);

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
                (self, manager) => self.status switch
                {
                    State.Loading => manager.max > 0
                        ? $"Loading...\n{manager.current} / {manager.max}"
                        : $"Loading...\n{manager.current}",
                    State.Offering => $"Offering items...\n{self.progress} / {self.maxProgress}",
                    _ => ""
                }
            )
            .ToProperty(this, x => x.StatusText);

        LoadCmd = ReactiveCommand.Create<Task>(LoadInventoryAsync);
        OfferItemsCmd = ReactiveCommand.Create<Task>(OfferItemsAsync);
        PlaceItemsCmd = ReactiveCommand.Create<Task>(PlaceItemsAsync);

        _inventoryManager.Loaded += OnInventoryLoaded;
        _inventoryManager.Cleared += OnInventoryCleared;
        _inventoryManager.ItemAdded += OnItemAdded;
        _inventoryManager.ItemRemoved += OnItemRemoved;
    }

    private Func<InventoryStackViewModel, bool> CreateFilter(string? filterText)
    {
        return (vm) => {
            if (string.IsNullOrWhiteSpace(filterText))
                return true;
            return vm.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        };
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

        Hotel currentHotel = Session.Hotel;

        _photoCache.Edit(cache => {
            cache.Clear();
            cache.AddOrUpdate(
                inventory
                    .OfType<IInventoryItem>()
                    .OfKind("external_image_wallitem_poster_small")
                    .Select(TryExtractPhotoId)
                    .Where(it => !string.IsNullOrWhiteSpace(it.PhotoId))
                    .Select(it => new InventoryPhotoViewModel(
                        it.Item,
                        new(() => FetchPhotoUrlAsync(currentHotel, it.PhotoId!))
                    ))
            );
        });

        ItemCount = inventory.Count;
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
    }

    private async Task OfferItemsAsync()
    {
        var viewModel = _dialogService.CreateViewModel<OfferItemsViewModel>();
        viewModel.Items = Selection.SelectedItems
            .Where(stack => stack is not null)
            .Select(stack => new OfferItemViewModel(stack!.Item) {
                Amount = stack.Count,
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

    private Task PlaceItemsAsync()
    {
        return Task.CompletedTask;
    }

    private async Task OfferItemsAsync(IEnumerable<OfferItemViewModel> offers)
    {
        List<IInventoryItem> toOffer = [];
        foreach (var offer in offers)
        {
            var maybeStack = _cache.Lookup((ItemDescriptor)offer.Item);
            if (!maybeStack.HasValue)
                return;
            var stack = maybeStack.Value;
            toOffer.Add(stack.Take(offer.Amount));
        }

        try
        {
            Progress = 0;
            MaxProgress = toOffer.Count;
            Status = State.Offering;

            if (Session.Is(ClientType.Origins))
                await OfferItemsOriginsAsync(toOffer);
            else
                await OfferItemsModernAsync(toOffer);
        }
        finally
        {
            Status = State.None;
            Progress = 0;
            MaxProgress = 0;
        }
    }

    private async Task OfferItemsOriginsAsync(List<IInventoryItem> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            Progress = i;
            if (i > 0)
                await Task.Delay(_config.Value.Timing.Origins.TradeOfferInterval);
            Send(new OfferTradeItemMsg(items[i]));
        }
    }

    private Task OfferItemsModernAsync(List<IInventoryItem> items)
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

        ItemCount = _inventoryManager.Inventory?.Count ?? 0;
    }

    public async Task LoadInventoryAsync()
    {
        try
        {
            Status = State.Loading;
            var items = await _inventoryManager.LoadInventoryAsync(timeout: 120000, forceReload: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Status = State.None;
        }
    }
}