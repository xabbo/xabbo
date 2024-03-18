using System;
using System.Collections.ObjectModel;

using ReactiveUI;
using DynamicData;
using DynamicData.Kernel;

using Xabbo.Core.Events;
using Xabbo.Core.Game;

using b7.Xabbo.Services;

namespace b7.Xabbo.Avalonia.ViewModels;

public class RoomEntitiesViewModel : ViewModelBase
{
    private readonly IUiContext _uiContext;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<EntityViewModel, int> _entityCache = new(x => x.Index);

    private readonly ReadOnlyObservableCollection<EntityViewModel> _entities;
    public ReadOnlyObservableCollection<EntityViewModel> Entities => _entities;

    public RoomEntitiesViewModel(IUiContext uiContext, RoomManager roomManager)
    {
        _uiContext = uiContext;
        _roomManager = roomManager;

        _entityCache.Connect().SortBy(x => x.Name).Bind(out _entities).Subscribe();

        _roomManager.Left += OnLeftRoom;
        _roomManager.EntityAdded += OnEntityAdded;
        _roomManager.EntityRemoved += OnEntityRemoved;
        _roomManager.EntityIdle += OnEntityIdle;
        _roomManager.EntityUpdated += OnEntityUpdated;
    }

    private void OnEntityUpdated(object? sender, EntityEventArgs e)
    {
        _entityCache.Lookup(e.Entity.Index).IfHasValue(vm =>
        {
            var currentUpdate = e.Entity.CurrentUpdate;
            if (currentUpdate is null) return;
            vm.IsTrading = currentUpdate.IsTrading;
        });
    }

    private void OnLeftRoom(object? sender, System.EventArgs e)
    {
        _uiContext.Invoke(_entityCache.Clear);
    }

    private void OnEntityAdded(object? sender, EntityEventArgs e)
    {
        _uiContext.Invoke(() => _entityCache.AddOrUpdate(new EntityViewModel(e.Entity)));
    }

    private void OnEntityRemoved(object? sender, EntityEventArgs e)
    {
        _uiContext.Invoke(() => _entityCache.RemoveKey(e.Entity.Index));
    }

    private void OnEntityIdle(object? sender, EntityIdleEventArgs e)
    {
        _entityCache.Lookup(e.Entity.Index).IfHasValue(vm =>
        {
            vm.IsIdle = e.Entity.IsIdle;
        });
    }
}

