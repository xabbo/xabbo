using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class RoomUsersConfig : ReactiveObject
{
    [Reactive] public bool ShowBots { get; set; } = true;
    [Reactive] public bool ShowPets { get; set; } = true;
}
