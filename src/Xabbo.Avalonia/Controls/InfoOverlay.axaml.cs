using Avalonia;
using Avalonia.Controls;

namespace Xabbo.Avalonia.Controls;

public class InfoOverlay : ContentControl
{
    public static readonly StyledProperty<string?> TextProperty
        = AvaloniaProperty.Register<InfoOverlay, string?>(nameof(Text));

    public static readonly StyledProperty<bool> ShowMessageProperty
        = AvaloniaProperty.Register<InfoOverlay, bool>(nameof(ShowMessage), defaultValue: true);

    public static readonly StyledProperty<bool> ShowContentProperty
        = AvaloniaProperty.Register<InfoOverlay, bool>(nameof(ShowContent), defaultValue: true);

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool ShowMessage
    {
        get => GetValue(ShowMessageProperty);
        set => SetValue(ShowMessageProperty, value);
    }

    public bool ShowContent
    {
        get => GetValue(ShowContentProperty);
        set => SetValue(ShowContentProperty, value);
    }
}