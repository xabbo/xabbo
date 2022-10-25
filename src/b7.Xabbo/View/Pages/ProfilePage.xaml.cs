using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class ProfilePage : Page
{
    public ProfilePage(WardrobeViewManager wardrobeViewManager)
    {
        DataContext = wardrobeViewManager;

        InitializeComponent();
    }
}
