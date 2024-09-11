using System;
using System.Collections.ObjectModel;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using DynamicData.Kernel;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;

using b7.Xabbo.Services;

namespace b7.Xabbo.Avalonia.ViewModels;

public class RoomAvatarsViewModel : ViewModelBase
{
    private readonly IUiContext _uiContext;
    private readonly RoomManager _roomManager;

    private readonly SourceCache<AvatarViewModel, int> _avatarCache = new(x => x.Index);

    private readonly ReadOnlyObservableCollection<AvatarViewModel> _avatars;
    public ReadOnlyObservableCollection<AvatarViewModel> Avatars => _avatars;

    [Reactive] public bool ShowPets { get; set; }
    [Reactive] public bool ShowBots { get; set; }
    [Reactive] public string FilterText { get; set; } = "";

    public RoomAvatarsViewModel(IUiContext uiContext, RoomManager roomManager)
    {
        _uiContext = uiContext;
        _roomManager = roomManager;

        _avatarCache
            .Connect()
            .Filter(FilterAvatar)
            .SortBy(x => x.Name)
            .Bind(out _avatars)
            .Subscribe();

        this.WhenAnyValue(x => x.FilterText).Subscribe(_ => _avatarCache.Refresh());

        _roomManager.Left += OnLeftRoom;
        _roomManager.AvatarAdded += OnAvatarAdded;
        _roomManager.AvatarRemoved += OnAvatarRemoved;
        _roomManager.AvatarIdle += OnAvatarIdle;
        _roomManager.AvatarUpdated += OnAvatarUpdated;
    }

    private bool FilterAvatar(AvatarViewModel avatar)
    {
        if (!ShowPets && avatar.Type == AvatarType.Pet)
            return false;
        if (!ShowBots && (avatar.Type == AvatarType.PublicBot || avatar.Type == AvatarType.PrivateBot))
            return false;
        if (!string.IsNullOrWhiteSpace(FilterText) && !avatar.Name.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase))
            return false;

        return true;
    }

    private void OnAvatarUpdated(AvatarEventArgs e)
    {
        _avatarCache.Lookup(e.Avatar.Index).IfHasValue(vm =>
        {
            var currentUpdate = e.Avatar.CurrentUpdate;
            if (currentUpdate is null) return;
            vm.IsTrading = currentUpdate.IsTrading;
        });
    }

    private void OnLeftRoom()
    {
        _uiContext.Invoke(_avatarCache.Clear);
    }

    private void OnAvatarAdded(AvatarEventArgs e)
    {
        _uiContext.Invoke(() => _avatarCache.AddOrUpdate(new AvatarViewModel(e.Avatar)));
    }

    private void OnAvatarRemoved(AvatarEventArgs e)
    {
        _uiContext.Invoke(() => _avatarCache.RemoveKey(e.Avatar.Index));
    }

    private void OnAvatarIdle(AvatarIdleEventArgs e)
    {
        _avatarCache.Lookup(e.Avatar.Index).IfHasValue(vm =>
        {
            vm.IsIdle = e.Avatar.IsIdle;
        });
    }
}

