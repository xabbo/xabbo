using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class TimingConfigs : ReactiveObject
{
    [Reactive] public ModernTimingConfig Modern { get; set; } = new();
    [Reactive] public OriginsTimingConfig Origins { get; set; } = new();

    public TimingConfigBase GetTiming(ClientType client) => client is ClientType.Origins ? Origins : Modern;
    public TimingConfigBase GetTiming(Session session) => GetTiming(session.Client.Type);
}