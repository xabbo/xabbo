using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class RoomConfig : ReactiveObject
{
    [Reactive] public bool RememberPasswordsConfirmed { get; set; }
    [Reactive] public bool RememberPasswords { get; set; }
    [Reactive] public bool AcceptFriendsAtDoor { get; set; }
}
