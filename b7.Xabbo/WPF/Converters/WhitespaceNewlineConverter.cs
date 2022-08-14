using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace b7.Xabbo.WPF.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class WhitespaceNewlineConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string s)
        {
            throw new ArgumentException("Input value must be a string.");
        }

        if (parameter is not int threshold)
        {
            threshold = 10;
        }

        return Regex.Replace(s, $@"[^\S\r\n]{{{threshold},}}", "\r\n");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
