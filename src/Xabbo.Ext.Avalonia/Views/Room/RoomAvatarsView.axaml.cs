using Avalonia.Collections;
using Avalonia.Controls;

namespace Xabbo.Ext.Avalonia.Views;

public partial class RoomAvatarsView : UserControl
{
    public RoomAvatarsView()
    {
        InitializeComponent();
        AvatarDataGrid.CopyingRowClipboardContent += AvatarDataGrid_CopyingRowClipboardContent;
    }

    private void AvatarDataGrid_CopyingRowClipboardContent(object? sender, DataGridRowClipboardEventArgs e)
    {
    }
}
