using System;

using GalaSoft.MvvmLight;

namespace b7.Xabbo.ViewModel;

public class BannedUserViewModel : ObservableObject
{
    public long Id { get; }
    public string Name { get; }

    public BannedUserViewModel(long id, string name)
    {
        Id = id;
        Name = name;
    }
}
