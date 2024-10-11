using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

using Xabbo.ViewModels;

namespace Xabbo.Avalonia.Views;

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

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (DataContext is not RoomFurniViewModel roomFurniViewModel)
            return;

        roomFurniViewModel.ContextSelection = DataGridFurni
            .SelectedItems
            .OfType<FurniViewModel>()
            .ToList();
    }
}
