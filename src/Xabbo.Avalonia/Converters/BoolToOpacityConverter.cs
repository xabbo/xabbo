using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Xabbo.Avalonia.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double trueValue = 1.00, falseValue = 0.00;
        if (parameter is string param)
        {
            string[] split = param.Split(';');
            if (split.Length == 2)
            {
                if (!double.TryParse(split[0], out trueValue))
                    trueValue = 1.0;
                if (!double.TryParse(split[1], out falseValue))
                    falseValue = 0.0;
            }
        }

        return value switch
        {
            bool b => b ? trueValue : falseValue,
            _ => falseValue,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
