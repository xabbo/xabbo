﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Reactive;
using DynamicData;
using DynamicData.Kernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Avalonia.Controls.Selection;
using FluentAvalonia.UI.Controls;
using Humanizer;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;

using Xabbo.Interceptor;
using Xabbo.Messages.Flash;
using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core.Messages.Outgoing;
using Xabbo.Services.Abstractions;

using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace Xabbo.ViewModels;

public sealed class FriendsPageViewModel : PageViewModel
{
    public override string Header => _cache.Count > 0 ? $"Friends ({_cache.Count})" : "Friends";
    public override IconSource? Icon { get; } = new SymbolIconSource { Symbol = Symbol.People };

    private readonly IUiContext _uiContext;
    private readonly IDialogService _dialogService;
    private readonly IInterceptor _interceptor;
    private readonly IFigureConverterService _figureConverter;
    private readonly FriendManager _friendManager;
    private readonly SourceCache<FriendViewModel, Id> _cache = new(key => key.Id);

    private readonly ReadOnlyObservableCollection<FriendViewModel> _friends;
    public ReadOnlyObservableCollection<FriendViewModel> Friends => _friends;

    [Reactive] public string FilterText { get; set; } = "";
    [Reactive] public bool ShowOnlineOnly { get; set; }

    public ReactiveCommand<FriendViewModel, Unit> FollowFriendCmd { get; }
    public ReactiveCommand<Unit, Unit> RemoveFriendsCmd { get; }

    public SelectionModel<FriendViewModel> Selection { get; } = new() { SingleSelect = false };

    public FriendsPageViewModel(
        IUiContext uiContext, IDialogService dialogService,
        IInterceptor interceptor, IFigureConverterService figureConverter,
        FriendManager friendManager)
    {
        _uiContext = uiContext;
        _dialogService = dialogService;
        _interceptor = interceptor;
        _figureConverter = figureConverter;
        _friendManager = friendManager;

        _figureConverter.Available += OnFigureConverterAvailable;

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

        FollowFriendCmd = ReactiveCommand.Create<FriendViewModel>(FollowFriend);
        RemoveFriendsCmd = ReactiveCommand.CreateFromTask(RemoveSelectedFriendsAsync);

        _friendManager.Loaded += OnFriendsLoaded;
        _friendManager.FriendAdded += OnFriendAdded;
        _friendManager.FriendUpdated += OnFriendUpdated;
        _friendManager.FriendRemoved += OnFriendRemoved;
    }

    private void OnFigureConverterAvailable()
    {
        foreach (var (_, vm) in _cache.KeyValues)
            UpdateOriginsFigure(vm);
    }

    private void UpdateOriginsFigure(FriendViewModel vm)
    {
        if (!vm.IsModernFigure &&
            _figureConverter.TryConvertToModern(vm.Figure, out Figure? figure))
        {
            using (vm.DelayChangeNotifications())
            {
                vm.Figure = figure.ToString();
                vm.IsModernFigure = true;
            }
        }
    }

    private FriendViewModel CreateViewModel(IFriend friend)
    {
        FriendViewModel vm = new()
        {
            Id = friend.Id,
            IsOnline = friend.IsOnline,
            Name = friend.Name,
            Motto = friend.Motto,
            Figure = friend.Figure,
            IsModernFigure = !_interceptor.Session.Hotel.IsOrigins
        };

        UpdateOriginsFigure(vm);

        return vm;
    }

    private void AddFriend(IFriend friend)
    {
        _cache.AddOrUpdate(CreateViewModel(friend));
        this.RaisePropertyChanged(nameof(Header));
    }

    private void UpdateFriend(IFriend friend)
    {
        _cache.Lookup(friend.Id).IfHasValue(vm => {
            using (vm.DelayChangeNotifications())
            {
                vm.IsOnline = friend.IsOnline;
                vm.Motto = friend.Motto;
                if (vm.Figure != friend.Figure)
                {
                    using (vm.DelayChangeNotifications())
                    {
                        vm.IsModernFigure = !_interceptor.Session.Hotel.IsOrigins;
                        vm.Figure = friend.Figure;
                        UpdateOriginsFigure(vm);
                    }
                }
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

    private void FollowFriend(FriendViewModel friend)
    {
        _interceptor.Send(Out.FollowFriend, friend.Id);
    }

    private async Task RemoveSelectedFriendsAsync()
    {
        List<FriendViewModel> friendsToRemove = Selection.SelectedItems
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        if (friendsToRemove.Count == 0)
            return;

        var result = await _dialogService.ShowContentDialogAsync(ViewModelLocator.Main, new ContentDialogSettings
        {
            Title = $"Delete {"friend".ToQuantity(friendsToRemove.Count)}",
            Content = $"Are you sure you wish to delete {
                friendsToRemove.Select(x => x.Name).Humanize(5, "more friends")
            }?",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
        });

        if (result is ContentDialogResult.Primary)
        {
            _interceptor.Send(new RemoveFriendsMsg(friendsToRemove.Select(x => x.Id)));
        }
    }
}