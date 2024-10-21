﻿using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using ReactiveUI;

using Xabbo.Controllers;
using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Extension;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public partial class RoomFurniViewModel : ViewModelBase
{
    [GeneratedRegex(@"^(?<name>.*?)(\bwhere:(?<expression>.*))?$")]
    private static partial Regex RegexExpression();

    private readonly RoomFurniController _furniController;
    private readonly IUiContext _uiCtx;
    private readonly IExtension _ext;
    private readonly ProfileManager _profileManager;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<FurniStackViewModel, ItemDescriptor> _furniStackCache = new(x => x.Descriptor);
    private readonly SourceCache<FurniViewModel, (ItemType, Id)> _furniCache = new(x => (x.Type, x.Id));

    private readonly ReadOnlyObservableCollection<FurniViewModel> _furni;
    private readonly ReadOnlyObservableCollection<FurniStackViewModel> _furniStacks;

    public ReadOnlyObservableCollection<FurniViewModel> Furni => _furni;
    public ReadOnlyObservableCollection<FurniStackViewModel> Stacks => _furniStacks;

    public RoomGiftsViewModel GiftsViewModel { get; }

    [Reactive] public string FilterText { get; set; } = "";
    [Reactive] public bool ShowGrid { get; set; }

    private readonly ObservableAsPropertyHelper<Func<FurniViewModel, bool>?> _nameFilter;
    public Func<FurniViewModel, bool>? NameFilter => _nameFilter.Value;

    private readonly ObservableAsPropertyHelper<Func<FurniViewModel, bool>?> _queryFilter;
    public Func<FurniViewModel, bool>? QueryFilter => _queryFilter.Value;

    private readonly ObservableAsPropertyHelper<string?> _queryFilterError;
    public string? QueryFilterError => _queryFilterError.Value;

    private readonly ObservableAsPropertyHelper<string?> _emptyStatus;
    public string? EmptyStatus => _emptyStatus.Value;

    private readonly ObservableAsPropertyHelper<string?> _emptyStatusGrid;
    public string? EmptyStatusGrid => _emptyStatusGrid.Value;

    [Reactive] public IList<FurniViewModel>? ContextSelection { get; set; }

    public ReactiveCommand<Unit, Unit> HideFurniCmd { get; }
    public ReactiveCommand<Unit, Unit> ShowFurniCmd { get; }
    public ReactiveCommand<Unit, Task> PickupCmd { get; }
    public ReactiveCommand<Unit, Task> EjectCmd { get; }
    public ReactiveCommand<Unit, Task> ToggleCmd { get; }
    public ReactiveCommand<Directions, Task> RotateCmd { get; }
    public ReactiveCommand<Unit, Task> MoveCmd { get; }
    public ReactiveCommand<Unit, Unit> CancelCmd { get; }

    private readonly ObservableAsPropertyHelper<bool> _isBusy;
    public bool IsBusy => _isBusy.Value;

    private readonly ObservableAsPropertyHelper<string> _statusText;
    public string StatusText => _statusText.Value;

    public RoomFurniViewModel(
        RoomFurniController furniController,
        IUiContext uiContext,
        IExtension extension,
        ProfileManager profileManager,
        RoomManager roomManager,
        RoomGiftsViewModel gifts)
    {
        _furniController = furniController;
        _uiCtx = uiContext;
        _ext = extension;
        _profileManager = profileManager;
        _roomManager = roomManager;

        GiftsViewModel = gifts;

        _roomManager.FurniVisibilityToggled += OnFurniVisibilityToggled;

        _roomManager.Left += OnLeftRoom;
        _roomManager.FloorItemsLoaded += OnFloorItemsLoaded;
        _roomManager.FloorItemAdded += OnFloorItemAdded;
        _roomManager.FloorItemUpdated += OnFloorItemUpdated;
        _roomManager.FloorItemDataUpdated += OnFloorItemDataUpdated;
        _roomManager.DiceUpdated += OnDiceUpdated;
        _roomManager.FloorItemWiredMovement += OnFloorItemWiredMovement;
        _roomManager.FloorItemRemoved += OnFloorItemRemoved;
        _roomManager.FloorItemSlide += OnFloorItemSlide;
        _roomManager.WallItemsLoaded += OnWallItemsLoaded;
        _roomManager.WallItemAdded += OnWallItemAdded;
        _roomManager.WallItemUpdated += OnWallItemUpdated;
        _roomManager.WallItemWiredMovement += OnWallItemWiredMovement;
        _roomManager.WallItemRemoved += OnWallItemRemoved;

        var nameAndQueryFilter = this.WhenAnyValue(
            x => x.FilterText,
            filterText => {
                string? nameFilterText = null, queryFilterText = null;
                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    var match = RegexExpression().Match(filterText);
                    if (match.Success)
                        (nameFilterText, queryFilterText) = (match.Groups["name"].Value, match.Groups["expression"].Value);
                }
                return (nameFilterText, queryFilterText);
            }
        );

        _nameFilter = nameAndQueryFilter
            .Select(x => CreateNameFilter(x.nameFilterText))
            .ToProperty(this, x => x.NameFilter);


        var queryFilterAndError = nameAndQueryFilter
            .Select(x => CreateQueryFilter(x.queryFilterText));

        _queryFilter = queryFilterAndError
            .Select(x => x.Filter)
            .ToProperty(this, x => x.QueryFilter);

        _queryFilterError = queryFilterAndError
            .Select(x => x.Error)
            .ToProperty(this, x => x.QueryFilterError);

        _furniCache
            .Connect()
            .Filter(
                this.WhenAnyValue(
                    x => x.NameFilter,
                    x => x.QueryFilter,
                    CombineFilters
                )
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _furni, SortExpressionComparer<FurniViewModel>.Ascending(x => x.Name))
            .Subscribe();

        _furniStackCache
            .Connect()
            .Filter(this.WhenAnyValue(x => x.FilterText).Select(CreateStackFilter))
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _furniStacks, SortExpressionComparer<FurniStackViewModel>.Ascending(x => x.Name))
            .Subscribe();

        _isBusy =
            _furniController.WhenAnyValue(
                x => x.CurrentOperation,
                op => op is not RoomFurniController.Operation.None
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.IsBusy);

        _statusText =
            _furniController.WhenAnyValue(
                x => x.CurrentOperation,
                x => x.CurrentProgress,
                x => x.TotalProgress,
                (op, current, total) => $"{op switch {
                    RoomFurniController.Operation.Eject => "Ejecting",
                    RoomFurniController.Operation.Pickup => "Picking up",
                    RoomFurniController.Operation.Toggle => "Toggling",
                    RoomFurniController.Operation.Rotate => "Rotating",
                    RoomFurniController.Operation.Move => "Click tiles to move",
                    _ => "Processing"
                }} furni..."
                + $"\n{current}/{total}"
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.StatusText);

        _emptyStatus =
            Observable.CombineLatest(
                _furniCache.CountChanged,
                _furni.WhenAnyValue(x => x.Count),
                this.WhenAnyValue(x => x.QueryFilterError),
                (actualCount, filteredCount, queryFilterError) => {
                    if (filteredCount > 0)
                        return null;
                    if (!string.IsNullOrWhiteSpace(queryFilterError))
                        return queryFilterError;
                    return actualCount == 0 ? "No furni in room" : "No furni matches";
                }
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.EmptyStatus);

        _emptyStatusGrid =
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.QueryFilter).Select(x => x is not null),
                _furniStackCache.CountChanged,
                _furniStacks.WhenAnyValue(x => x.Count),
                (isQuery, actualCount, filteredCount) =>
                    filteredCount > 0 ? null :
                    actualCount == 0
                    ? "No furni in room"
                    : (isQuery ? "Grid view does not support query filters" : "No furni matches")
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.EmptyStatusGrid);

        HideFurniCmd = ReactiveCommand.Create(
            HideFurni,
            this
                .WhenAnyValue(
                    x => x.ContextSelection,
                    selection => selection?.Any(it => !it.IsHidden) == true
                )
                .ObserveOn(RxApp.MainThreadScheduler)
        );

        ShowFurniCmd = ReactiveCommand.Create(
            ShowFurni,
            this
                .WhenAnyValue(
                    x => x.ContextSelection,
                    selection => selection?.Any(it => it.IsHidden) == true
                )
                .ObserveOn(RxApp.MainThreadScheduler)
        );

        PickupCmd = ReactiveCommand.Create(
            PickupAsync,
            Observable.CombineLatest(
                _ext.WhenAnyValue(x => x.Session),
                _roomManager.WhenAnyValue(x => x.IsOwner),
                _profileManager.WhenAnyValue(Xabbo => Xabbo.UserData),
                this.WhenAnyValue(x => x.ContextSelection),
                _furniController.WhenAnyValue(x => x.CurrentOperation),
                (session, isOwner, userData, selection, op) =>
                    op is RoomFurniController.Operation.None &&
                    session.Is(ClientType.Shockwave)
                        ? isOwner && selection?.Any() == true
                        : selection?.Any(it => it.OwnerId == userData?.Id) == true
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        EjectCmd = ReactiveCommand.Create(
            EjectAsync,
            Observable.CombineLatest(
                _ext.WhenAnyValue(x => x.Session),
                _roomManager.WhenAnyValue(x => x.IsOwner),
                _profileManager.WhenAnyValue(Xabbo => Xabbo.UserData),
                this.WhenAnyValue(x => x.ContextSelection),
                _furniController.WhenAnyValue(x => x.CurrentOperation),
                (session, isOwner, userData, selection, op) =>
                    op is RoomFurniController.Operation.None &&
                    session.Is(ClientType.Modern) &&
                    isOwner &&
                    userData is not null &&
                    selection?.Any(it => it.OwnerId != userData.Id) == true
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        ToggleCmd = ReactiveCommand.Create(
            ToggleAsync,
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.ContextSelection),
                _roomManager.WhenAnyValue(x => x.RightsLevel),
                _furniController.WhenAnyValue(
                    x => x.CurrentOperation,
                    op => op is RoomFurniController.Operation.None
                ),
                (selection, rights, isNotBusy) =>
                    isNotBusy &&
                    rights > RightsLevel.None &&
                    selection is { Count: > 0 }
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        RotateCmd = ReactiveCommand.Create<Directions, Task>(
            RotateAsync,
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.ContextSelection),
                _roomManager.WhenAnyValue(x => x.RightsLevel),
                _furniController.WhenAnyValue(
                    x => x.CurrentOperation,
                    op => op is RoomFurniController.Operation.None
                ),
                (selection, rights, isNotBusy) =>
                    isNotBusy &&
                    rights > RightsLevel.None &&
                    selection?.Any(it => it.IsFloorItem) == true
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        MoveCmd = ReactiveCommand.Create<Task>(
            MoveAsync,
            Observable.CombineLatest(
                this.WhenAnyValue(x => x.ContextSelection),
                _roomManager.WhenAnyValue(x => x.RightsLevel),
                _furniController.WhenAnyValue(
                    x => x.CurrentOperation,
                    op => op is RoomFurniController.Operation.None
                ),
                (selection, rights, isNotBusy) =>
                    isNotBusy &&
                    rights > RightsLevel.None &&
                    selection?.Any(it => it.IsFloorItem) == true
            )
            .ObserveOn(RxApp.MainThreadScheduler)
        );

        CancelCmd = ReactiveCommand.Create(_furniController.CancelCurrentOperation);
    }

    private Task ToggleAsync() => ContextSelection is { } selection
        ? _furniController.ToggleFurniAsync(selection.Select(x => x.Furni))
        : Task.CompletedTask;

    private Task RotateAsync(Directions direction) => ContextSelection is { } selection
        ? _furniController.RotateFurniAsync(selection.Select(x => x.Furni), direction)
        : Task.CompletedTask;

    private Task MoveAsync() => ContextSelection is { } selection
        ? _furniController.MoveFurniAsync(selection.Select(x => x.Furni))
        : Task.CompletedTask;

    private Task PickupAsync()
    {
        if (ContextSelection is { } selection)
        {
            return _furniController.PickupFurniAsync(selection.Select(x => x.Furni));
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    private Task EjectAsync()
    {
        if (ContextSelection is { } selection)
        {
            return _furniController.EjectFurniAsync(selection.Select(x => x.Furni));
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    private void HideFurni()
    {
        if (ContextSelection is { } selection)
        {
            foreach (var vm in selection)
            {
                _roomManager.HideFurni(vm.Furni);
            }
        }
    }

    private void ShowFurni()
    {
        if (ContextSelection is { } selection)
        {
            foreach (var vm in selection)
            {
                _roomManager.ShowFurni(vm.Furni);
            }
        }
    }

    private void OnFurniVisibilityToggled(FurniEventArgs e)
    {
        _furniCache
            .Lookup((e.Furni.Type, e.Furni.Id))
            .IfHasValue(vm => vm.IsHidden = e.Furni.IsHidden);
    }

    static Func<FurniViewModel, bool> CreateNameFilter(string? nameFilter)
    {
        if (string.IsNullOrWhiteSpace(nameFilter))
            return static (vm) => true;

        return (vm) =>
            string.IsNullOrWhiteSpace(nameFilter) ||
            vm.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase);
    }

    static (Func<FurniViewModel, bool>? Filter, string? Error) CreateQueryFilter(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return (null, null);

        try
        {
            return (
                DynamicExpressionParser.
                ParseLambda<FurniViewModel, bool>(
                    new ParsingConfig(), false, expression).Compile(),
                null
            );
        }
        catch (Exception ex)
        {
            return ((vm) => false, ex.Message);
        }
    }

    static Func<FurniViewModel, bool> CombineFilters(Func<FurniViewModel, bool>? a, Func<FurniViewModel, bool>? b)
    {
        return (vm) => a?.Invoke(vm) != false && b?.Invoke(vm) != false;
    }

    static Func<FurniStackViewModel, bool> CreateStackFilter(string? filterText)
    {
        return (vm) =>
            string.IsNullOrWhiteSpace(filterText) ||
            vm.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase);
    }

    private void ClearItems()
    {
        _uiCtx.Invoke(() => {
            _furniCache.Clear();
            _furniStackCache.Clear();
        });
    }

    private void AddItems(IEnumerable<IFurni> items)
    {
        _uiCtx.Invoke(() =>
        {
            _furniCache.Edit(cache =>
            {
                foreach (var item in items)
                {
                    cache.AddOrUpdate(new FurniViewModel(item));
                }
            });
            _furniStackCache.Edit(cache =>
            {
                foreach (var item in items)
                {
                    var key = item.GetDescriptor();
                    cache
                        .Lookup(key)
                        .IfHasValue(vm => vm.Count++)
                        .Else(() => cache.AddOrUpdate(new FurniStackViewModel(key)));
                }
            });
        });
    }

    private void RemoveItem(IFurni item)
    {
        _uiCtx.Invoke(() => {
            _furniCache.RemoveKey((item.Type, item.Id));
            var desc = item.GetDescriptor();
            _furniStackCache.Lookup(desc).IfHasValue(vm =>
            {
                vm.Count--;
                if (vm.Count == 0)
                {
                    _furniStackCache.RemoveKey(desc);
                }
            });
        });
    }

    private void OnLeftRoom() => ClearItems();

    private void UpdateFurni(IFurni furni) => _furniCache.Lookup((furni.Type, furni.Id))
        .IfHasValue(vm => {
            vm.Item = furni;
            if (QueryFilter is not null)
                _furniCache.Refresh();
        });

    private void OnFloorItemsLoaded(FloorItemsEventArgs e) => AddItems(e.Items);
    private void OnFloorItemAdded(FloorItemEventArgs e) => AddItems([e.Item]);
    private void OnFloorItemUpdated(FloorItemUpdatedEventArgs e) => UpdateFurni(e.Item);
    private void OnFloorItemDataUpdated(FloorItemDataUpdatedEventArgs e) => UpdateFurni(e.Item);
    private void OnDiceUpdated(DiceUpdatedEventArgs e) => UpdateFurni(e.Item);
    private void OnFloorItemSlide(FloorItemSlideEventArgs e) => UpdateFurni(e.Item);
    private void OnFloorItemWiredMovement(FloorItemWiredMovementEventArgs e) => UpdateFurni(e.Item);
    private void OnFloorItemRemoved(FloorItemEventArgs e) => RemoveItem(e.Item);
    private void OnWallItemsLoaded(WallItemsEventArgs e) => AddItems(e.Items);
    private void OnWallItemAdded(WallItemEventArgs e) => AddItems([e.Item]);
    private void OnWallItemUpdated(WallItemUpdatedEventArgs e) => UpdateFurni(e.Item);
    private void OnWallItemWiredMovement(WallItemWiredMovementEventArgs e) => UpdateFurni(e.Item);
    private void OnWallItemRemoved(WallItemEventArgs e) => RemoveItem(e.Item);
}
