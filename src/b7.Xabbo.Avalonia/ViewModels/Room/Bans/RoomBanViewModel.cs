namespace b7.Xabbo.Avalonia.ViewModels;

public class RoomBanViewModel(long id, string name) : ViewModelBase
{
    public long Id { get; } = id;
    public string Name { get; } = name;
}
