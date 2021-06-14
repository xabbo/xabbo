using System;
using System.Globalization;
using System.Windows.Data;

namespace b7.Xabbo.WPF.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double trueValue = 1.00, falseValue = 0.00;
            if (parameter is string param)
            {
                string[] split = param.Split(new char[] { ';' });
                if (split.Length != 2)
                    throw new FormatException();
                trueValue = double.Parse(split[0]);
                falseValue = double.Parse(split[1]);
            }

            return ((bool)value) ? trueValue : falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
