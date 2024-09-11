using System.ComponentModel;

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Xabbo.Ext.Avalonia.Views;

public partial class RoomFurniListView : UserControl
{
    public RoomFurniListView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        DataGridFurni.Columns[1].Sort(ListSortDirection.Ascending);
    }
}
