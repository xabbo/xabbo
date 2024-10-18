using ReactiveUI;

namespace Xabbo.ViewModels;

public abstract class ChatLogEntryViewModel : ViewModelBase
{
    public long EntryId { get; set; }
    public DateTime Timestamp { get; } = DateTime.Now;

    public virtual bool IsSelectable => true;
    private bool _isSelected;
    public bool IsSelected
    {
        get => IsSelectable && _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, IsSelectable && value);
    }
}
