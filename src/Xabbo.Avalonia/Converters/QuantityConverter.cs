using System;
using System.Globalization;
using Avalonia.Data.Converters;

using Humanizer;

namespace Xabbo.Avalonia.Converters;

public sealed class QuantityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string name)
        {
            return value switch
            {
                int x => name.ToQuantity(x),
                long x => name.ToQuantity(x),
                double x => name.ToQuantity(x),
                _ => value?.ToString()
            };
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

}