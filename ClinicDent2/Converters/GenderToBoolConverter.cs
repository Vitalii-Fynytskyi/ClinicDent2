using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(string), typeof(bool))]
    class GenderToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string gender = (string)value;
            if(parameter.ToString() == "Male" && gender == "Чол")
            {
                return true;
            }
            if(parameter.ToString() == "Male" && gender=="Жін")
            {
                return false;
            }
            if (parameter.ToString() == "Female" && gender == "Чол")
            {
                return false;
            }
            if (parameter.ToString() == "Female" && gender == "Жін")
            {
                return true;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (parameter.ToString() == "Male" && isChecked == true)
            {
                return "Чол";
            }
            if (parameter.ToString() == "Male" && isChecked == false)
            {
                return "Жін";
            }
            if (parameter.ToString() == "Female" && isChecked == true)
            {
                return "Жін";
            }
            if (parameter.ToString() == "Female" && isChecked == false)
            {
                return "Чол";
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
