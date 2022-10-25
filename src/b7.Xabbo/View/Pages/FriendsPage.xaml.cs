using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class FriendsPage : Page
{
    public FriendsPage(FriendListViewManager manager)
    {
        DataContext = manager;

        InitializeComponent();
    }
}
