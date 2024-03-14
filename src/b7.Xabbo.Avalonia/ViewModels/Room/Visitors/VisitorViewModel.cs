using System;
using ReactiveUI.Fody.Helpers;

namespace b7.Xabbo.Avalonia.ViewModels;

public class VisitorViewModel(int index, long id, string name) : ViewModelBase
{
    public int Index { get; set; } = index;
    public long Id { get; } = id;
    public string Name { get; } = name;

    [Reactive] public DateTime? Entered { get; set; }
    [Reactive] public DateTime? Left { get; set; }
    [Reactive] public int Visits { get; set; } = 1;
}
