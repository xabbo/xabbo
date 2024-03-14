using FluentAvalonia.UI.Controls;

namespace b7.Xabbo.Avalonia.ViewModels;

public abstract class PageViewModel : ViewModelBase
{
    public abstract string Header { get; }
    public abstract IconSource? Icon { get; }
}
