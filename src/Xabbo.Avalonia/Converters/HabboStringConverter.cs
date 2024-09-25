using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

using Xabbo.Core;

namespace Xabbo.Avalonia.Converters;

public class HabboStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        string text => H.RenderText(text),
        _ => new BindingNotification(new ArgumentException("Value is not a string"), BindingErrorType.Error)
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}
