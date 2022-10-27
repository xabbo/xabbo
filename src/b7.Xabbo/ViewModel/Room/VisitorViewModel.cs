using System;

using CommunityToolkit.Mvvm.ComponentModel;

namespace b7.Xabbo.ViewModel;

public class VisitorViewModel : ObservableObject
{
    public int Index { get; set; }
    public long Id { get; }
    public string Name { get; }

    private DateTime? _entered;
    public DateTime? Entered
    {
        get => _entered;
        set => SetProperty(ref _entered, value);
    }

    private DateTime? _left;
    public DateTime? Left
    {
        get => _left;
        set => SetProperty(ref _left, value);
    }

    private int _visits;
    public int Visits
    {
        get => _visits;
        set => SetProperty(ref _visits, value);
    }

    public VisitorViewModel(int index, long id, string name)
    {
        Index = index;
        Id = id;
        Name = name;
        Visits = 1;
    }
}
