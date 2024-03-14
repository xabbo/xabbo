using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace b7.Xabbo.Avalonia.Converters;

// [ValueConversion(typeof(string), typeof(string))]
public class WhitespaceNewlineConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s)
            return new BindingNotification(new ArgumentException("Input value must be a string."), BindingErrorType.Error);

        if (parameter is not int threshold)
            threshold = 10;

        return Regex.Replace(s, $@"[^\S\r\n]{{{threshold},}}", "\r\n");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
