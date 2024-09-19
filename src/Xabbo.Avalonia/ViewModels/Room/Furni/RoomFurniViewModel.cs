using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Kernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;
using Xabbo.Core.GameData;
using Xabbo.Services.Abstractions;

namespace Xabbo.ViewModels;

public class RoomFurniViewModel : ViewModelBase
{
    private readonly IUiContext _uiCtx;
    private readonly IGameDataManager _gameData;
    private readonly RoomManager _roomManager;

    private readonly Dictionary<ItemDescriptor, FurniStackViewModel> _stackMap = [];
    private readonly SourceCache<FurniStackViewModel, StackDescriptor> _furniStackCache = new(x => x.Descriptor);
    private readonly SourceCache<FurniViewModel, (ItemType, long)> _furniCache = new(x => (x.Type, x.Id));

    private readonly ReadOnlyObservableCollection<FurniViewModel> _furni;
    private readonly ReadOnlyObservableCollection<FurniStackViewModel> _furniStacks;

    public ReadOnlyObservableCollection<FurniViewModel> Furni => _furni;
    public ReadOnlyObservableCollection<FurniStackViewModel> Stacks => _furniStacks;

    [Reactive] public string FilterText { get; set; } = "";

    [Reactive] public bool ShowGrid { get; set; }

    public RoomFurniViewModel(IUiContext uiContext, IGameDataManager gameData, RoomManager roomManager)
    {
        _uiCtx = uiContext;
        _gameData = gameData;
        _roomManager = roomManager;

        _roomManager.FurniVisibilityToggled += OnFurniVisibilityToggled;

        _roomManager.Left += OnLeftRoom;
        _roomManager.FloorItemsLoaded += OnFloorItemsLoaded;
        _roomManager.FloorItemAdded += OnFloorItemAdded;
        _roomManager.FloorItemRemoved += OnFloorItemRemoved;

        _furniCache
            .Connect()
            .Filter(FilterFurni)
            .SortBy(x => x.Name)
            .Bind(out _furni)
            .Subscribe();

        _furniStackCache
            .Connect()
            .Filter(FilterFurniStack)
            .SortBy(x => x.Name)
            .Bind(out _furniStacks)
            .Subscribe();

        this.WhenAnyValue(x => x.FilterText).Subscribe(_ =>
        {
            _furniCache.Refresh();
            _furniStackCache.Refresh();
        });
    }

    private void OnFurniVisibilityToggled(FurniEventArgs e)
    {
        _furniCache
            .Lookup((e.Item.Type, e.Item.Id))
            .IfHasValue(vm => vm.IsHidden = e.Item.IsHidden);
    }

    private bool FilterFurni(FurniViewModel vm)
    {
        return vm.Name.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase);
    }

    private bool FilterFurniStack(FurniStackViewModel vm)
    {
        return vm.Name.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase);
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
                    var vm = new FurniStackViewModel(item);
                    cache.Lookup(vm.Descriptor).IfHasValue(vm => vm.Count++).Else(() => cache.AddOrUpdate(vm));
                }
            });
        });
    }

    private void RemoveItem(IFurni item)
    {
        _uiCtx.Invoke(() => {
            _furniCache.RemoveKey((item.Type, item.Id));
            var desc = FurniStackViewModel.GetDescriptor(item);
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

    private void OnFloorItemsLoaded(FloorItemsEventArgs e)
    {
        AddItems(e.Items);
    }

    private void OnFloorItemAdded(FloorItemEventArgs e)
    {
        AddItems([e.Item]);
    }

    private void OnFloorItemRemoved(FloorItemEventArgs e)
    {
        RemoveItem(e.Item);
    }
}
