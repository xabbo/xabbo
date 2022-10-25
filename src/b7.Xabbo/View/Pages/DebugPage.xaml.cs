using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class DebugPage : Page
{
    public DebugPage(DebugViewManager manager)
    {
        DataContext = manager;

        InitializeComponent();
    }
}
