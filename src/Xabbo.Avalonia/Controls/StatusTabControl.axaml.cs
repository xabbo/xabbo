using Avalonia;
using Avalonia.Controls;

namespace Xabbo.Avalonia.Controls;

public class StatusTabControl : TabControl
{
    public static readonly StyledProperty<string?> StatusProperty
        = AvaloniaProperty.Register<StatusTabControl, string?>(nameof(Status));

    public string? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}