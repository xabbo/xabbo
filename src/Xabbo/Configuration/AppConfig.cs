using ReactiveUI;

namespace Xabbo.Configuration;

public sealed partial class AppConfig : ReactiveObject
{
    [Reactive] public GeneralConfig General { get; set; } = new();
    [Reactive] public GameConfig Game { get; set; } = new();
    [Reactive] public RoomConfig Room { get; set; } = new();
    [Reactive] public MovementConfig Movement { get; set; } = new();
    [Reactive] public ChatConfig Chat { get; set; } = new();

    [Reactive] public TimingConfigs Timing { get; set; } = new();

    [Reactive] public ViewConfig View { get; set; } = new();
}
