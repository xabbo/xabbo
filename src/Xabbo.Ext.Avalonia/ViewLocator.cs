using HanumanInstitute.MvvmDialogs.Avalonia;

using Xabbo.Ext.Avalonia.Views;
using Xabbo.Ext.Avalonia.ViewModels;

namespace Xabbo.Ext.Avalonia;

public class ViewLocator : StrongViewLocator
{
    public ViewLocator()
    {
        Register<MainViewModel, MainWindow>();
    }
}
