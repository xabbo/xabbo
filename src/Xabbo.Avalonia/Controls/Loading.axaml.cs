using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Xabbo.Avalonia.Controls;

public class Loading : ContentControl
{
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Loading, bool>(nameof(IsLoading), defaultValue: false);

    public static readonly StyledProperty<object?> LoadingContentProperty =
        AvaloniaProperty.Register<Loading, object?>(nameof(LoadingContent));

    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<Loading, ICommand?>(nameof(CancelCommandProperty));

    public static readonly StyledProperty<TimeSpan> FadeDurationProperty =
        AvaloniaProperty.Register<Loading, TimeSpan>(nameof(FadeDurationProperty), defaultValue: TimeSpan.FromSeconds(0.5));

    public static readonly DirectProperty<Loading, TimeSpan?> FadeInDurationProperty =
        AvaloniaProperty.RegisterDirect<Loading, TimeSpan?>(
            nameof(FadeInDurationProperty),
            control => control.FadeInDuration ?? control.FadeDuration,
            (control, value) => control.FadeInDuration = value
        );

    public static readonly DirectProperty<Loading, TimeSpan?> FadeOutDurationProperty =
        AvaloniaProperty.RegisterDirect<Loading, TimeSpan?>(
            nameof(FadeOutDurationProperty),
            control => control.FadeOutDuration ?? control.FadeDuration,
            (control, value) => control.FadeOutDuration = value
        );

    public static readonly StyledProperty<double> ContentFadeOpacityProperty =
        AvaloniaProperty.Register<Loading, double>(nameof(ContentFadeOpacityProperty), defaultValue: 0);

    private static TimeSpan? CoerceFade(AvaloniaObject @object, TimeSpan? nullable)
        => nullable is { } value ? value : @object.GetValue(FadeDurationProperty);

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

    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    public TimeSpan FadeDuration
    {
        get => GetValue(FadeDurationProperty);
        set => SetValue(FadeDurationProperty, value);
    }

    private TimeSpan? _fadeInDuration;
    public TimeSpan? FadeInDuration
    {
        get => _fadeInDuration;
        set => SetAndRaise(FadeInDurationProperty, ref _fadeInDuration, value);
    }

    private TimeSpan? _fadeOutDuration;
    public TimeSpan? FadeOutDuration
    {
        get => _fadeOutDuration;
        set => SetAndRaise(FadeOutDurationProperty, ref _fadeOutDuration, value);
    }

    public double ContentFadeOpacity
    {
        get => GetValue(ContentFadeOpacityProperty);
        set => SetValue(ContentFadeOpacityProperty, value);
    }
}