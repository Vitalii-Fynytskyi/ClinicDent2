using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;
using System.Collections;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(ICollection), typeof(Visibility))]
    public class CollectionCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
