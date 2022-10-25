using System;

using GalaSoft.MvvmLight;

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
        set => Set(ref _entered, value);
    }

    private DateTime? _left;
    public DateTime? Left
    {
        get => _left;
        set => Set(ref _left, value);
    }

    private int _visits;
    public int Visits
    {
        get => _visits;
        set => Set(ref _visits, value);
    }

    public VisitorViewModel(int index, long id, string name)
    {
        Index = index;
        Id = id;
        Name = name;
        Visits = 1;
    }
}
