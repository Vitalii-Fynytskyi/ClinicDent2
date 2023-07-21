using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(byte[]), typeof(BitmapImage))]
    class BytesToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] bytes = value as byte[];
            if(bytes == null)
            {
                return DependencyProperty.UnsetValue;
            }
            if (bytes.Length == 0)
            {
                return DependencyProperty.UnsetValue;
            }
            MemoryStream ms = new MemoryStream(bytes);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
