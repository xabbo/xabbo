namespace Xabbo.ViewModels;

public class VisitorViewModel(int index, long id, string name) : ViewModelBase
{
    public long Id { get; } = id;
    public string Name { get; } = name;

    [Reactive] public int Index { get; set; } = index;
    [Reactive] public DateTime? Entered { get; set; }
    [Reactive] public DateTime? Left { get; set; }
    [Reactive] public int Visits { get; set; } = 1;
}
