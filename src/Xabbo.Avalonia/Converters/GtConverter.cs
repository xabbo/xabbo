using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Xabbo.Avalonia.Converters;

public class GtConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double n = parameter switch
        {
            int v => v,
            float v => v,
            double v => v,
            _ => 0,
        };

        return value switch
        {
            int v => v > n,
            float v => v > n,
            double v => v > n,
            _ => false
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
