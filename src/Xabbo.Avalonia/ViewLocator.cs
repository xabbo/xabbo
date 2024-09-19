using HanumanInstitute.MvvmDialogs.Avalonia;

using Xabbo.Views;
using Xabbo.ViewModels;

namespace Xabbo;

public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<MainViewModel, MainWindow>();
    }
}
