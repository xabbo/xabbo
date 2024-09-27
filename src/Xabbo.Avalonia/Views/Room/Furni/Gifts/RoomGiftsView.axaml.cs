using Avalonia;
using Avalonia.Controls;

using Xabbo.ViewModels;

namespace Xabbo.Avalonia.Views;

public partial class RoomGiftsView : UserControl
{
    public RoomGiftsView()
    {
        InitializeComponent();
    }

    void OnContextRequested(object? source, ContextRequestedEventArgs e)
    {
        if (DataContext is not RoomGiftsViewModel gifts)
            return;

        if (e.Source is IDataContextProvider { DataContext: GiftViewModel gift })
        {
            gifts.TargetGift = gift;
        }
        else
        {
            gifts.TargetGift = null;
        }
    }
}
