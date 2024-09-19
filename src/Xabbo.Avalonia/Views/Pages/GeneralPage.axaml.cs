using Avalonia.Controls;

using Xabbo.ViewModels;

using ReactiveUI;

namespace Xabbo.Views;

public partial class GeneralPage : UserControl, IViewFor<GeneralPageViewModel>
{
    public GeneralPageViewModel? ViewModel { get; set; }
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = value as GeneralPageViewModel; }

    public GeneralPage()
    {
        InitializeComponent();
    }
}
