using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Учет.Enums;

namespace Учет.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AssetStatus status)
            {
                return status switch
                {
                    AssetStatus.Registered => new SolidColorBrush(Color.FromRgb(200, 230, 201)),
                    AssetStatus.InService => Brushes.White,
                    AssetStatus.UnderMaintenance => new SolidColorBrush(Color.FromRgb(255, 249, 196)),
                    AssetStatus.UnderRepair => new SolidColorBrush(Color.FromRgb(255, 224, 178)),
                    AssetStatus.Expired => new SolidColorBrush(Color.FromRgb(255, 138, 128)),
                    AssetStatus.WrittenOff => new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}