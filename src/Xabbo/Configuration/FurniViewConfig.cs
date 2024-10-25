using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class FurniViewConfig : ReactiveObject
{
    [Reactive] public int RefreshIntervalMs { get; set; } = 500;
}