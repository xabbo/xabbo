using ReactiveUI;

namespace Xabbo.Configuration;

public sealed class ViewConfig : ReactiveObject
{
    [Reactive] public FurniViewConfig Furni { get; set; } = new();
}