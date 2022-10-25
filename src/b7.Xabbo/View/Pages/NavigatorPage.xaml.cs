using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class NavigatorPage : Page
{
    public NavigatorPage(NavigatorViewManager manager)
    {
        DataContext = manager;

        InitializeComponent();
    }
}
