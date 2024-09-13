using Avalonia.Controls;

using Xabbo.Ext.Avalonia.ViewModels;

using ReactiveUI;

namespace Xabbo.Ext.Avalonia.Views;

public partial class GeneralPage : UserControl, IViewFor<GeneralPageViewModel>
{
    public GeneralPageViewModel? ViewModel { get; set; }
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as GeneralPageViewModel; }

    public GeneralPage()
    {
        InitializeComponent();
    }
}
