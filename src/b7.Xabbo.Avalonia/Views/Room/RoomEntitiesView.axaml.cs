using Avalonia.Collections;
using Avalonia.Controls;

namespace b7.Xabbo.Avalonia.Views;

public partial class RoomEntitiesView : UserControl
{
    public RoomEntitiesView()
    {
        InitializeComponent();
        EntityDataGrid.CopyingRowClipboardContent += EntityDataGrid_CopyingRowClipboardContent;
    }

    private void EntityDataGrid_CopyingRowClipboardContent(object? sender, DataGridRowClipboardEventArgs e)
    {
    }
}
