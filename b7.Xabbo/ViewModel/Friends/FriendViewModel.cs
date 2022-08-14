using System.Web;

using CommunityToolkit.Mvvm.ComponentModel;

using Xabbo.Core;

namespace b7.Xabbo.ViewModel;

public class FriendViewModel : ObservableObject
{
    public IFriend Friend { get; set; }

    public long Id => Friend.Id;
    public string Name => Friend.Name;

    private bool _isOnline;
    public bool IsOnline
    {
        get => _isOnline;
        set => SetProperty(ref _isOnline, value);
    }

    public string ImageUrl { get; }

    public FriendViewModel(IFriend friend)
    {
        Friend = friend;
        _isOnline = friend.IsOnline;

        ImageUrl = $"https://www.habbo.com/habbo-imaging/avatarimage?user={HttpUtility.UrlEncode(Name)}&direction=2&head_direction=2&headonly=1";
    }
}
