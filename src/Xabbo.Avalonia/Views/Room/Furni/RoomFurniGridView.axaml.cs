using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Xabbo.Avalonia.Views;

public partial class RoomFurniGridView : UserControl
{
    public RoomFurniGridView()
    {
        InitializeComponent();

        FurniStacks.AttachedToVisualTree += OnFurniStacksAttached;
    }

    private void OnFurniStacksAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // Workaround for a weird bug where items have zero dimensions when switching to the furni grid view.
        Dispatcher.UIThread.InvokeAsync(() => FurniStacks.InvalidateMeasure());
    }
}
