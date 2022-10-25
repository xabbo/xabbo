using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace b7.Xabbo.WPF.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => ((bool)value) ? Visibility.Visible : (parameter != null ? Visibility.Collapsed : Visibility.Hidden);
   
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => ((Visibility)value) == Visibility.Visible;
}
