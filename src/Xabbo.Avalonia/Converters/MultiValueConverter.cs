using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Xabbo.Avalonia.Converters;

public class MultiValueConverter : List<IValueConverter>, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => new BindingNotification(new NotImplementedException(), BindingErrorType.Error);
}
