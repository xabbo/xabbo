using FluentAvalonia.UI.Controls;

namespace Xabbo.Ext.Avalonia.ViewModels;

public abstract class PageViewModel : ViewModelBase
{
    public abstract string Header { get; }
    public abstract IconSource? Icon { get; }
}
