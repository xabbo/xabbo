using System;

using CommunityToolkit.Mvvm.ComponentModel;

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
