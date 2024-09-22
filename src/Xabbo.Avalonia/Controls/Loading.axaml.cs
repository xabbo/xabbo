using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Xabbo.Avalonia.Controls;

public class Loading : ContentControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Loading, bool>(nameof(IsLoading), defaultValue: false);

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<Loading, object?>(nameof(LoadingContent));

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public object? LoadingContent
    {
        get => GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }
}
