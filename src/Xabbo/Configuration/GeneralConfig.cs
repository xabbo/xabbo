using System.Text.Json.Serialization;
using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class GeneralConfig : ReactiveObject
{
    [Reactive] public bool AntiTyping { get; set; } = true;
    [Reactive] public bool AntiIdle { get; set; } = true;
    [Reactive] public bool AntiIdleOut { get; set; } = true;
    [Reactive] public bool AntiTrade { get; set; }
    [Reactive] public bool AntiTradeCloseTrade { get; set; }

    [Reactive] public bool ClickToIgnoresFriends { get; set; } = true;
    [Reactive] public int BounceUnbanDelay { get; set; } = 100;

    [JsonIgnore, Reactive] public bool PrivacyMode { get; set; }
}
