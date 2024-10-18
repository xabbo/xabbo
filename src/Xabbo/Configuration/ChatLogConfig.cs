using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class ChatLogConfig : ReactiveObject
{
    [Reactive] public bool Normal { get; set; } = true;
    [Reactive] public bool Whispers { get; set; } = true;
    [Reactive] public bool Wired { get; set; } = false;
    [Reactive] public bool Bots { get; set; } = false;
    [Reactive] public bool Pets { get; set; } = false;
    [Reactive] public bool Trades { get; set; } = true;
    [Reactive] public bool UserEntry { get; set; } = false;

    [Reactive] public bool LogToFile { get; set; } = false;
}
