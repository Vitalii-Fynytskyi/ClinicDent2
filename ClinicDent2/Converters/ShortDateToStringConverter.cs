using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(string), typeof(DateTime))]
    class ShortDateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (string.IsNullOrEmpty(strValue))
            {
                return DateTime.Today;
            }
            DateTime resultDateTime;
            if (DateTime.TryParseExact(strValue,Options.DatePattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out resultDateTime))
            {
                return resultDateTime;
            }
            return DependencyProperty.UnsetValue;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? selectedDate = (DateTime?)value;
            if (selectedDate == null)
            {
                return DependencyProperty.UnsetValue;
            }
            return selectedDate.GetValueOrDefault().ToString(Options.DatePattern);
        }
    }
}
