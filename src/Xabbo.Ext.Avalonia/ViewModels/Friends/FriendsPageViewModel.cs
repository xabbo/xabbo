using System;
using System.Collections.ObjectModel;
using DynamicData;
using DynamicData.Kernel;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Interceptor;

using Xabbo.Ext.Services;

using IconSource = FluentAvalonia.UI.Controls.IconSource;
using System.Reactive;
using Xabbo.Messages.Flash;

namespace Xabbo.Ext.Avalonia.ViewModels;

public sealed class FriendsPageViewModel : PageViewModel
{
    public override string Header => _cache.Count > 0 ? $"Friends ({_cache.Count})" : "Friends";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.People };

    private readonly IUiContext _uiContext;
    private readonly IInterceptor _interceptor;
    private readonly FriendManager _friendManager;
    private readonly SourceCache<FriendViewModel, Id> _cache = new(key => key.Id);

    private readonly ReadOnlyObservableCollection<FriendViewModel> _friends;
    public ReadOnlyObservableCollection<FriendViewModel> Friends => _friends;

    [Reactive] public string FilterText { get; set; } = "";
    [Reactive] public bool ShowOnlineOnly { get; set; }

    public ReactiveCommand<FriendViewModel, Unit> FollowFriendCmd { get; }

    public FriendsPageViewModel(IUiContext uiContext, IInterceptor interceptor, FriendManager friendManager)
    {
        _cache
            .Connect()
            .Filter(friend => {
                if (ShowOnlineOnly && !friend.IsOnline) return false;
                if (!string.IsNullOrWhiteSpace(FilterText)
                    && friend.Name.Contains(FilterText, StringComparison.CurrentCultureIgnoreCase))
                    return false;
                return true;
            })
            .SortBy(x => x.Name)
            .Bind(out _friends)
            .Subscribe();

        this
            .WhenAnyValue(x => x.FilterText, x => x.ShowOnlineOnly)
            .Subscribe(_ => _cache.Refresh());

        FollowFriendCmd = ReactiveCommand.Create<FriendViewModel>(
            (FriendViewModel friend) =>
            _interceptor.Send(Out.FollowFriend, friend.Id));

        _uiContext = uiContext;
        _interceptor = interceptor;
        _friendManager = friendManager;
        _friendManager.Loaded += OnFriendsLoaded;
        _friendManager.FriendAdded += OnFriendAdded;
        _friendManager.FriendUpdated += OnFriendUpdated;
        _friendManager.FriendRemoved += OnFriendRemoved;
    }

    private void AddFriend(IFriend friend)
    {
        _cache.AddOrUpdate(new FriendViewModel
        {
            Hotel = _interceptor.Session.Hotel,
            Id = friend.Id,
            IsOnline = friend.IsOnline,
            Name = friend.Name,
            Motto = friend.Motto,
            Figure = friend.Figure,
        });
        this.RaisePropertyChanged(nameof(Header));
    }

    private void UpdateFriend(IFriend friend)
    {
        _cache.Lookup(friend.Id).IfHasValue(vm => {
            using (vm.DelayChangeNotifications())
            {
                vm.IsOnline = friend.IsOnline;
                vm.Motto = friend.Motto;
                vm.Figure = friend.Figure;
            }
        });
    }

    private void RemoveFriend(Id id)
    {
        _cache.RemoveKey(id);
        this.RaisePropertyChanged(nameof(Header));
    }

    private void OnFriendsLoaded()
    {
        foreach (var friend in _friendManager.Friends)
            AddFriend(friend);
    }
    private void OnFriendAdded(FriendEventArgs args) => AddFriend(args.Friend);
    private void OnFriendUpdated(FriendEventArgs args) => UpdateFriend(args.Friend);
    private void OnFriendRemoved(FriendEventArgs args) => RemoveFriend(args.Friend.Id);
}
