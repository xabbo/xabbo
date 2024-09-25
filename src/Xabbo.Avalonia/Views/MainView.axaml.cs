using Avalonia.Controls;
using Avalonia.Interactivity;

using Xabbo.ViewModels;

namespace Xabbo.Avalonia.Views;

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
        if (ViewModel is null) return;

        if (e.InvokedItemContainer.DataContext is PageViewModel pageVm)
            ViewModel.SelectedPage = pageVm;
    }
}
