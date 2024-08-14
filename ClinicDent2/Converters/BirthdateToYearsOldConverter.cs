using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    class BirthdateToYearsOldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string birthdateStr = (string)value;

            DateTime birthdate;
            if (DateTime.TryParse(birthdateStr, out birthdate) == true)
            {
                DateTime dateToCompare = DateTime.Now;
                int countYears = 0;
                while (dateToCompare.AddYears(-1) >= birthdate)
                {
                    countYears = countYears + 1;
                    dateToCompare = dateToCompare.AddYears(-1);
                }
                return String.Format("{0} років", countYears);
            }

            int birthYear;
            if(int.TryParse(birthdateStr, out birthYear) == true)
            {
                int countYears = DateTime.Now.Year - birthYear;
                return String.Format("{0} років", countYears);
            }
            return "? років";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
