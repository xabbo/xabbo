using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

using Xabbo.Models;

namespace Xabbo.Avalonia.Converters;

public sealed class HslU8Converter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is HslU8 color)
        {
            return new HslColor(1, color.H / 255.0 * 360.0, color.S / 255.0, color.L / 255.0);
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        HslColor? hsl = value switch
        {
            HslColor c => c,
            HsvColor c => c.ToHsl(),
            Color c => c.ToHsl(),
            _ => null
        };

        if (hsl is null) return null;

        return new HslU8(
            (byte)(hsl.Value.H / 360.0 * 255.0),
            (byte)(hsl.Value.S * 255.0),
            (byte)(hsl.Value.L * 255.0)
        );
    }
}