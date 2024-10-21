using HanumanInstitute.MvvmDialogs;
using Xabbo.Core;

namespace Xabbo.ViewModels;

public class OfferItemsViewModel : ViewModelBase, IModalDialogViewModel
{
    public bool? DialogResult => null;

    public List<OfferItemViewModel> Items { get; set; } = [];
}

public class OfferItemViewModel(IItem item) : ItemViewModelBase(item)
{
    [Reactive] public int Amount { get; set; }
    [Reactive] public int MinAmount { get; set; }
    [Reactive] public int MaxAmount { get; set; }
}