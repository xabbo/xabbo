using System;
using System.Linq;
using System.Windows.Controls;

using b7.Xabbo.ViewModel;

namespace b7.Xabbo.View
{
    public partial class RoomFurniView : UserControl
    {
        public RoomFurniView()
        {
            InitializeComponent();
        }

        private void listViewFurni_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FurniViewManager? viewManager = DataContext as FurniViewManager;
            if (viewManager is null) return;

            viewManager.RefreshCommandsCanExecute();
        }

        private void listViewFurni_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FurniViewManager? viewManager = DataContext as FurniViewManager;
            if (viewManager is null) return;

            viewManager.SelectedItems = ((ListView)sender).SelectedItems.Cast<FurniViewModel>().ToList();
        }
    }
}
