using Avalonia.Controls;
using Avalonia.Interactivity;

using Xabbo.Ext.Avalonia.ViewModels;

namespace Xabbo.Ext.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainViewModel? ViewModel => DataContext as MainViewModel;

    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        NavView.ItemInvoked += NavView_ItemInvoked;
    }

    private void NavView_ItemInvoked(object? sender, FluentAvalonia.UI.Controls.NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer.DataContext is PageViewModel pageVm)
        {
            if (ViewModel is not null)
            {
                ViewModel.CurrentPage = pageVm;
            }
        }
    }
}
