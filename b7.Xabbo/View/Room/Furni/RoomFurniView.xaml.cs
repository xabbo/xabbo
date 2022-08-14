namespace b7.Xabbo.View; 

using System.Linq;
using System.Windows.Controls;

using b7.Xabbo.ViewModel;

public partial class RoomFurniView : UserControl
{
    public RoomFurniView()
    {
        InitializeComponent();
    }

    private void listViewFurni_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (DataContext is not FurniViewManager viewManager) return;

        viewManager.RefreshCommandsCanExecute();
    }

    private void listViewFurni_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView ||
            DataContext is not FurniViewManager viewManager) return;

        viewManager.SelectedItems = listView.SelectedItems.Cast<FurniViewModel>().ToList();
    }
}
