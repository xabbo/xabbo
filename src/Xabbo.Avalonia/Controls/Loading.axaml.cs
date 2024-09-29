using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace Xabbo.Avalonia.Controls;

public class Loading : ContentControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Loading, bool>(nameof(IsLoading), defaultValue: false);

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<Loading, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<ICommand?> CancelProperty =
        AvaloniaProperty.Register<Loading, ICommand?>(nameof(CancelProperty));

    public static readonly StyledProperty<TimeSpan> FadeDurationProperty =
        AvaloniaProperty.Register<Loading, TimeSpan>(nameof(FadeDurationProperty), defaultValue: TimeSpan.FromSeconds(0.5));

    public static readonly StyledProperty<TimeSpan?> FadeInDurationProperty =
        AvaloniaProperty.Register<Loading, TimeSpan?>(nameof(FadeInDurationProperty), coerce: CoerceFade);

    public static readonly StyledProperty<TimeSpan?> FadeOutDurationProperty =
        AvaloniaProperty.Register<Loading, TimeSpan?>(nameof(FadeOutDurationProperty), coerce: CoerceFade);

    public static readonly StyledProperty<double> ContentFadeOpacityProperty =
        AvaloniaProperty.Register<Loading, double>(nameof(ContentFadeOpacityProperty), defaultValue: 0);

    private static TimeSpan? CoerceFade(AvaloniaObject @object, TimeSpan? nullable)
        => nullable is { } value ? value : @object.GetValue(FadeDurationProperty);

    public TimeSpan FadeDuration
    {
        get => GetValue(FadeDurationProperty);
        set => SetValue(FadeDurationProperty, value);
    }

    public TimeSpan? FadeInDuration
    {
        get => GetValue(FadeInDurationProperty) ?? GetValue(FadeDurationProperty);
        set => SetValue(FadeInDurationProperty, value);
    }

    public TimeSpan? FadeOutDuration
    {
        get => GetValue(FadeOutDurationProperty) ?? GetValue(FadeDurationProperty);
        set => SetValue(FadeOutDurationProperty, value);
    }

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

    public double ContentFadeOpacity
    {
        get => GetValue(ContentFadeOpacityProperty);
        set => SetValue(ContentFadeOpacityProperty, value);
    }

    public ICommand? Cancel
    {
        get => GetValue(CancelProperty);
        set => SetValue(CancelProperty, value);
    }
}