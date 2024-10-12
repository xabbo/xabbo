using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Xabbo.Avalonia.Converters;

public class RoundingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Console.WriteLine(value);
        Console.WriteLine(parameter);
        if (parameter is int frequency)
        {
            int integerValue = value switch
            {
                int v => v,
                float v => (int)v,
                double v => (int)v,
                _ => throw new Exception($"Invalid value type: {value?.GetType()}")
            };

            return (integerValue + (frequency / 2)) / frequency * frequency;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}