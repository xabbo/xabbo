using HanumanInstitute.MvvmDialogs.Avalonia;

using Xabbo.Avalonia.Views;
using Xabbo.ViewModels;

namespace Xabbo.Avalonia;

public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<MainViewModel, MainWindow>();
        Register<OfferItemsViewModel, OfferItemsView>();
    }
}
