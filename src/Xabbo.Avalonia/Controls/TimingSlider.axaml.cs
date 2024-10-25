using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Xabbo.Avalonia.Controls;

public class TimingSlider : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<TimingSlider, string>(nameof(Text));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<TimingSlider, string>(nameof(Description));

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<TimingSlider, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<TimingSlider, double>(nameof(Minimum), defaultValue: 0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<TimingSlider, double>(nameof(Maximum), defaultValue: 2000);

    public static readonly StyledProperty<double> TickFrequencyProperty =
        AvaloniaProperty.Register<TimingSlider, double>(nameof(TickFrequency), defaultValue: 10);

    public static readonly StyledProperty<double> VisualTickFrequencyProperty =
        AvaloniaProperty.Register<TimingSlider, double>(nameof(VisualTickFrequency), defaultValue: 100);

    public static readonly StyledProperty<string?> TipProperty =
        AvaloniaProperty.Register<TimingSlider, string?>(nameof(Tip));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    public double VisualTickFrequency
    {
        get => GetValue(VisualTickFrequencyProperty);
        set => SetValue(VisualTickFrequencyProperty, value);
    }

    public string? Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }
}