using ClinicDentClientCommon.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ClinicDent2.Converters
{
    [ValueConversion(typeof(AssetType), typeof(string))]
    internal class StageAssetTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((AssetType)value)
            {
                case AssetType.Bond:
                    return "Бонд";
                case AssetType.Dentin:
                    return "Дентин";
                case AssetType.Enamel:
                    return "Емаль";
                case AssetType.CanalMethod:
                    return "Метод каналів";
                case AssetType.Sealer:
                    return "Силер";
                case AssetType.Cement:
                    return "Цемент";
                case AssetType.Technician:
                    return "Технік";
                case AssetType.Pin:
                    return "Штифт";
                case AssetType.Operation:
                    return "Операція";
                case AssetType.Calcium:
                    return "Кальцій";
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "Бонд":
                    return AssetType.Bond;
                case "Дентин":
                    return AssetType.Dentin;
                case "Емаль":
                    return AssetType.Enamel;
                case "Метод каналів":
                    return AssetType.CanalMethod;
                case "Силер":
                    return AssetType.Sealer;
                case "Цемент":
                    return AssetType.Cement;
                case "Технік":
                    return AssetType.Technician;
                case "Штифт":
                    return AssetType.Pin;
                case "Операція":
                    return AssetType.Operation;
                case "Кальцій":
                    return AssetType.Calcium;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
