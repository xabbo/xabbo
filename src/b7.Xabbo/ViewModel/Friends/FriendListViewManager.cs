using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using b7.Xabbo.Services;
using CommunityToolkit.Mvvm.ComponentModel;

using Xabbo.Core;
using Xabbo.Core.Events;
using Xabbo.Core.Game;

namespace b7.Xabbo.ViewModel;

public class FriendListViewManager : ObservableObject
{
    private readonly IUiContext _uiContext;
    private readonly FriendManager _friendManager;

    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<FriendViewModel> Friends { get; } = new();

    public FriendListViewManager(IUiContext uiContext, FriendManager friends)
    {
        _uiContext = uiContext;
        _friendManager = friends;

        _friendManager.Loaded += OnFriendsLoaded;
        _friendManager.FriendAdded += OnFriendAdded;
        _friendManager.FriendUpdated += OnFriendUpdated;
        _friendManager.FriendRemoved += OnFriendRemoved;
    }

    private void OnFriendsLoaded(object? sender, EventArgs e)
    {
        _uiContext.Invoke(() =>
        {
            foreach (IFriend friend in _friendManager.Friends)
                Friends.Add(new FriendViewModel(friend));
        });

        IsLoading = false;
    }

    private void OnFriendAdded(object? sender, FriendEventArgs e)
    {
        _uiContext.Invoke(() =>
        {
            Friends.Add(new FriendViewModel(e.Friend));
        });
    }

    private void OnFriendUpdated(object? sender, FriendUpdatedEventArgs e)
    {
        // TODO
    }

    private void OnFriendRemoved(object? sender, FriendEventArgs e)
    {
        _uiContext.Invoke(() =>
        {
            var friendViewModel = Friends.FirstOrDefault(x => x.Id == e.Friend.Id);
            if (friendViewModel is not null)
                Friends.Remove(friendViewModel);
        });
    }
}
