using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace luxury_cars.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            bool flag = value is true;

            // If ConverterParameter=Inverse → flip the bool
            if (parameter is string p && p == "Inverse")
                flag = !flag;

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
            => value is Visibility.Visible;
    }
}