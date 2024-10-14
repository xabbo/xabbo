using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Xabbo.Avalonia.Converters;

public sealed class FirstLetterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && text.Length > 0)
            return text[0].ToString().ToUpper();
        else
            return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}