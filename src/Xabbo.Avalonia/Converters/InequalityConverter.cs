using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Xabbo.Avalonia.Converters;

public class InequalityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !EqualityComparer<object?>.Default.Equals(value, parameter);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
