using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View.Pages;

public partial class GeneralPage : Page
{
    public GeneralPage(GeneralViewManager manager)
    {
        DataContext = manager;

        InitializeComponent();
    }
}
