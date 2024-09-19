using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class GeneralConfig : ReactiveObject
{
    [Reactive] public bool AntiTyping { get; set; } = true;
    [Reactive] public bool AntiIdle { get; set; } = true;
    [Reactive] public bool AntiIdleOut { get; set; } = true;
    [Reactive] public bool AntiTrade { get; set; } = false;

    [Reactive] public bool ClickToIgnoresFriends { get; set; } = true;
    [Reactive] public int BounceUnbanDelay { get; set; } = 100;
}
