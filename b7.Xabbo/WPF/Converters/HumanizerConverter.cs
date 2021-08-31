using System;
using System.Globalization;
using System.Windows.Data;

using Humanizer;

namespace b7.Xabbo.WPF.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class HumanizerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Humanize() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
