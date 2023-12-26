using ClinicDent2.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(ImageType), typeof(string))]
    class ImageTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            ImageType imageType = (ImageType)value;
            if (imageType == ImageType.All)
            {
                return "Всі";
            }
            if (imageType == ImageType.Regular)
            {
                return "Звичайні";
            }
            if (imageType == ImageType.XRay)
            {
                return "Рентгени";
            }
            if (imageType == ImageType.Undefined)
            {
                return "Невизначені";
            }
            return DependencyProperty.UnsetValue;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }
            string s = (string)value;
            switch (s)
            {
                case "Всі":
                    return ImageType.All;
                case "Рентгени":
                    return ImageType.XRay;
                case "Звичайні":
                    return ImageType.Regular;
                case "Невизначені":
                    return ImageType.Undefined;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }
    }
}
