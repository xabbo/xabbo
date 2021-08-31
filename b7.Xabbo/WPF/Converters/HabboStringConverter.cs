using System;
using System.Globalization;
using System.Windows.Data;

using Xabbo.Core;

namespace b7.Xabbo.WPF.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class HabboStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
                throw new ArgumentException("Value is not a string.");

            return H.RenderText(text);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
