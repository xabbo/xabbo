using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Xabbo.Avalonia.Controls;

public class TimingSlider : TemplatedControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<TimingSlider, string>(nameof(Text));

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<TimingSlider, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> TipProperty =
        AvaloniaProperty.Register<TimingSlider, string?>(nameof(Tip));

    public static readonly StyledProperty<int> RoundingProperty =
        AvaloniaProperty.Register<TimingSlider, int>(nameof(Rounding), defaultValue: 10);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string? Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    public int Rounding
    {
        get => GetValue(RoundingProperty);
        set => SetValue(RoundingProperty, value);
    }
}