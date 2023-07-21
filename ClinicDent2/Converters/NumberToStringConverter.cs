using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    class NumberToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }
            else
            {
                return value.ToString();
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return 0;
            }
            int number;
            if (int.TryParse((string)value, out number) == true)
            {
                return number;
            }
            else
            {
                return 0;
            }
        }
    }
}
