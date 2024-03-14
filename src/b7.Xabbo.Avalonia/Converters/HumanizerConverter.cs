using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Humanizer;

namespace b7.Xabbo.Avalonia.Converters;

// [ValueConversion(typeof(object?), typeof(string))]
public class HumanizerConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        Enum e => e.Humanize(),
        _ => value?.ToString().Humanize() ?? string.Empty
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
