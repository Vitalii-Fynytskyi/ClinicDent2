using System;
using System.Globalization;
using System.Windows.Data;

namespace ClinicDent2.Converters
{
    class PatientsViewDotsVisibilityConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2) return false;

            if (values[0] is int selectedPage && values[1] is int countPages)
            {
                if (parameter.ToString() == "prefix")
                {
                    if(countPages != 0 && selectedPage>8)
                    {
                        return true;
                    }
                }
                else
                {
                    if(countPages != 0 && selectedPage + 7 < countPages)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
