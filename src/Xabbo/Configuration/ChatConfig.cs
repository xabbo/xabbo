using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class ChatConfig : ReactiveObject
{
    [Reactive] public bool MutePets { get; set; }
    [Reactive] public bool MutePetCommands { get; set; }
    [Reactive] public bool MuteBots { get; set; }
    [Reactive] public bool MuteWired { get; set; }
    [Reactive] public bool MuteRespects { get; set; }
    [Reactive] public bool MuteScratches { get; set; }
    [Reactive] public HashSet<string> PetCommands { get; set; } = [];
    [Reactive] public ChatLogSettings Log { get; set; } = new();
}