using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class ChatConfig : ReactiveObject
{
    [Reactive] public bool AlwaysShout { get; set; }
    [Reactive] public bool MuteAll { get; set; }
    [Reactive] public bool MutePets { get; set; } = true;
    [Reactive] public bool MutePetCommands { get; set; } = true;
    [Reactive] public bool MuteBots { get; set; } = true;
    [Reactive] public bool MuteWired { get; set; }
    [Reactive] public bool MuteRespects { get; set; }
    [Reactive] public bool MuteScratches { get; set; }
    [Reactive] public ChatLogConfig Log { get; set; } = new();
}